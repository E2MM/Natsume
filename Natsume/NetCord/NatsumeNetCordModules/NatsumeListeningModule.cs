using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Natsume.OpenAI.Models;
using Natsume.OpenAI.NatsumeIntelligence;
using Natsume.Persistence.Contact;
using Natsume.Utils;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Natsume.NetCord.NatsumeNetCordModules;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public class NatsumeListeningModule(
    NatsumeIntelligenceService natsumeIntelligenceService,
    RestClient client,
    IServiceProvider serviceProvider,
    TimeProvider timeProvider,
    ILogger<NatsumeListeningModule> logger
) : IGatewayEventHandler<Message>
{
    private static int _interactionId;

    public async ValueTask HandleAsync(Message message) // TODO: cancellation token source missing??
    {
        using var loggingScope = logger.BeginScope($"📎 #{++_interactionId:D6} ");

        try
        {
            using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(HandleAsync), 60_000);

            // TODO: cancellation token source??
            var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(90));
            using var scope = serviceProvider.CreateScope();
            var natsumeContactService = scope.ServiceProvider.GetRequiredService<NatsumeContactService>();

            var contact = await natsumeContactService.GetNatsumeContactByIdAsync(
                discordId: message.Author.Id,
                cancellationToken: cts.Token
            ) ?? await natsumeContactService.AddNatsumeContactAsync(
                discordId: message.Author.Id,
                discordNickname: message.Author.GetName(),
                cancellationToken: cts.Token
            );

            if (contact is { IsFriend: false } or { CurrentFavor: < 0 }) return;

            var natsumeListeningContext = new NatsumeListeningContext(
                natsumeDiscordUser: await client.GetCurrentUserAsync(cancellationToken: cts.Token),
                message: message,
                channel: message.Channel ??
                         await client.GetChannelAsync(channelId: message.ChannelId, cancellationToken: cts.Token)
            );

            var react = HandleNatsumeReactions(natsumeListeningContext, natsumeContactService, contact);
            var reply = HandleNatsumeReply(natsumeListeningContext, natsumeContactService, contact);

            //var reactions = NatsumeMightReact(natsumeListeningContext, natsumeContactService);
            //var replies = NatsumeMightReply(natsumeListeningContext, natsumeContactService);

            await Task.WhenAll(react, reply);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error: {Message}", e.Message);
        }
    }

    private bool IsNatsumeReacting(NatsumeListeningContext context)
    {
        var likelihood = context switch
        {
            { IsOwnMessage: true } => -1,
            { IsEveryoneTagged: true } => 1,
            { IsNatsumeExplicitlyTagged: true } => 1,
            { IsNatsumeTagged: true } => 1,
            { IsDirectMessage: true } => (context.MessageLength + 250) / 1_000d,
            { IsDirectMessage: false } => (context.MessageLength * context.MessageLength + 15_000) / 500_000d,
        };

        return Random.Shared.NextDouble() < likelihood;
    }

    private bool IsNatsumeReplying(NatsumeListeningContext context)
    {
        var likelihood = context switch
        {
            { IsOwnMessage: true } => -1,
            { IsEveryoneTagged: true } => 0.6,
            { IsNatsumeExplicitlyTagged: true } => 1,
            { IsNatsumeTagged: true } => 0.6,
            { IsDirectMessage: true } => 1,
            { IsDirectMessage: false } => (context.MessageLength * context.MessageLength + 25_000) / 2_250_000d,
        };

        return Random.Shared.NextDouble() < likelihood;
    }

    private async Task AddNatsumeReactions(NatsumeListeningContext context, List<string> reactions)
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(AddNatsumeReactions), 60_000);

        foreach (var reaction in reactions)
        {
            try
            {
                await Task.Delay(Random.Shared.Next(4500, 15000));
                await context.Message.AddReactionAsync(new ReactionEmojiProperties(reaction));
            }
            catch
            {
                logger.LogWarning("Natsume's reaction \"{Reaction}\" is not a valid Discord reaction", reaction);
            }
        }
    }

    private async Task HandleNatsumeReactions(
        NatsumeListeningContext context,
        NatsumeContactService natsumeContactService,
        NatsumeContact contact
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(HandleNatsumeReactions), 60_000);

        if (IsNatsumeReacting(context))
        {
            var (reactions, generationCost) =
                await natsumeIntelligenceService.GenerateNatsumeReactionsAsync(
                    messageContent: context.Message.Content
                );

            var updateContactTask = natsumeContactService.UpdateNatsumeContactsAsync(
                cancellationToken: default,
                contact.Interact().ExpendFavorForFriendship(generationCost)
            );

            var addReactionsTask = AddNatsumeReactions(
                context: context,
                reactions: reactions
            );

            await Task.WhenAll(updateContactTask, addReactionsTask);
        }
    }

    private async Task HandleNatsumeReply(
        NatsumeListeningContext context,
        NatsumeContactService natsumeContactService,
        NatsumeContact contact
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(HandleNatsumeReply), 15_000);

        if (IsNatsumeReplying(context))
        {
            using var typingReminder = await NatsumeIsTypingScope(context);

            var conversationMessages = await FetchAllConversationMessagesAsync(context);
            var openAiChatMessages = GenerateChatMessages(context, conversationMessages);

            var (generatedReply, generationCost) = await natsumeIntelligenceService.GenerateNatsumeReplyAsync(
                messages: openAiChatMessages);

            var updateContactTask = natsumeContactService.UpdateNatsumeContactsAsync(
                cancellationToken: default,
                contact.Interact().ExpendFavorForFriendship(generationCost)
            );

            var replyTask = NatsumeReplies(context, generatedReply);

            await Task.WhenAll(updateContactTask, replyTask);
        }
    }

    private async Task<IDisposable> NatsumeIsTypingScope(NatsumeListeningContext context)
    {
        using var timedScope = logger.BeginTimedOperationScope(
            timeProvider: timeProvider,
            operation: nameof(NatsumeIsTypingScope),
            warningThresholdMilliseconds: 1_000
        );

        try
        {
            logger.LogInformation("'Message.Channel?' is {NotNull}null", context.Message.Channel is null ? "" : "not ");
            var channel = context.Message.Channel ?? await client.GetChannelAsync(context.Message.ChannelId);
            if (channel is TextChannel textChannel) return await textChannel.EnterTypingStateAsync();
            throw new Exception("Channel is not a text channel");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error: {message}", e.Message);;
            return Task.FromException(e);
        }
    }

    private async Task<List<RestMessage>> FetchAllConversationMessagesAsync(NatsumeListeningContext context)
    {
        using var timedScope =
            logger.BeginTimedOperationScope(timeProvider, nameof(FetchAllConversationMessagesAsync), 4_000);

        var channel = context.Message.Channel ?? await client.GetChannelAsync(context.Message.ChannelId);

        var maxTotalChars = context switch
        {
            { IsNatsumeExplicitlyTagged: true } or { IsDirectMessage: true } => Math.Min(20_000,
                context.MessageLength * 10 + 4_000),
            { IsEveryoneTagged: true } or { IsNatsumeTagged: true } => Math.Min(12_500,
                context.MessageLength * 8 + 2_500),
            { IsDirectMessage: false } => Math.Min(7_500, context.MessageLength * 5 + 1_500),
        };

        var maxTotalMessages = 100;

        var totalChars = 0;

        List<RestMessage> discordMessages = [context.Message];
        RestMessage referencedMessage = context.Message;
        while (referencedMessage.ReferencedMessage is not null && totalChars < maxTotalChars)
        {
            referencedMessage =
                await client.GetMessageAsync(referencedMessage.ChannelId, referencedMessage.ReferencedMessage.Id);
            discordMessages.Add(referencedMessage);

            totalChars += referencedMessage.Content.Length;
        }

        logger.LogInformation("Total referenced messages: {DiscordMessagesCount}", discordMessages.Count);

        if (channel is TextChannel textChannel)
        {
            var previousMessages = textChannel.GetMessagesAsync(new PaginationProperties<ulong>()
                .WithDirection(direction: PaginationDirection.Before)
                .WithLimit(maxTotalMessages)
                .WithFrom(context.Message.Id)
            );

            await foreach (var m in previousMessages)
            {
                if (totalChars > maxTotalChars) break;

                discordMessages.Add(m);
                totalChars += m.Content.Length;
            }

            logger.LogInformation("Total chars: {TotalChars}", totalChars);
        }

        var orderedHistory = discordMessages
            .DistinctBy(m => m.Id)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        logger.LogInformation("Total messages: {OrderedHistoryCount}", orderedHistory.Count);

        return orderedHistory;
    }

    private (ChatMessageType type, string content) GetChatMessage(
        NatsumeListeningContext context,
        RestMessage message
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(GetChatMessage), 100);

        var messageContent = message.Content;
        foreach (var user in message.MentionedUsers)
        {
            messageContent = message.Content.Replace($"<@{user.Id}>", user.GetName());
        }

        if (message.Author != context.NatsumeDiscordUser) // && message.Author != context.Message.Author)
        {
            messageContent = $"""
                              Mittente: {message.Author.GetName()}
                              Messaggio:
                              {messageContent}
                              """;
        }

        return (message.Author == context.NatsumeDiscordUser) switch
        {
            true => (ChatMessageType.Assistant, messageContent),
            false => (ChatMessageType.User, messageContent)
        };
    }

    private List<(ChatMessageType type, string content)> GenerateChatMessages(
        NatsumeListeningContext context,
        List<RestMessage> messages
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(GenerateChatMessages), 1_000);

        var prompt = context switch
        {
            { IsNatsumeExplicitlyTagged: true } or { IsDirectMessage: true } => NatsumePrompt.SystemChat,
            _ => NatsumePrompt.SystemQuickChat
        };

        List<(ChatMessageType type, string content)> chatMessages =
        [
            (ChatMessageType.System, prompt)
        ];

        chatMessages.AddRange(messages.Select(m => GetChatMessage(context, m)));

        return chatMessages;
    }

    private async Task NatsumeReplies(NatsumeListeningContext context, string completion)
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(NatsumeReplies), 30_000);

        using var typingReminder = await NatsumeIsTypingScope(context);
        var splits = completion.SplitForDiscord();
        await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(splits[0]));

        foreach (var split in splits[1..])
        {
            await Task.Delay(Random.Shared.Next(2500, 5000));
            await context.Message.SendAsync(new MessageProperties().WithContent(split));
        }
    }
}