using Microsoft.Extensions.DependencyInjection;
using Natsume.NatsumeIntelligence;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.OpenAI.Models;
using Natsume.OpenAI.Prompts;
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
    TimeProvider timeProvider
) : IGatewayEventHandler<Message>
{
    public async ValueTask HandleAsync(Message message)
    {
        try
        {
            using var timingScope = new TimingScope(timeProvider, nameof(HandleAsync));

            // TODO: cancellation token source??
            var cts = new CancellationTokenSource(delay: TimeSpan.FromSeconds(60));
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
            //if (contact.IsFriend is false || contact.CurrentFavor < 0) return;

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
            Console.WriteLine(e);
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
        using var timingScope = new TimingScope(timeProvider, nameof(AddNatsumeReactions));

        foreach (var reaction in reactions)
        {
            try
            {
                await Task.Delay(Random.Shared.Next(4500, 15000));
                await context.Message.AddReactionAsync(new ReactionEmojiProperties(reaction));
            }
            catch
            {
                Console.WriteLine($"Natsume's reaction \"{reaction}\" is not a valid Discord reaction");
            }
        }
    }

    private async Task HandleNatsumeReactions(
        NatsumeListeningContext context,
        NatsumeContactService natsumeContactService,
        NatsumeContact contact
    )
    {
        using var timingScope = new TimingScope(timeProvider, nameof(HandleNatsumeReactions));

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
        using var timingScope = new TimingScope(timeProvider, nameof(HandleNatsumeReply));

        if (IsNatsumeReplying(context))
        {
            using var typingReminder = await NatsumeStartsTyping(context);

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

    private async Task<IDisposable> NatsumeStartsTyping(NatsumeListeningContext context)
    {
        using var timingScope = new TimingScope(timeProvider, nameof(NatsumeStartsTyping));

        // TODO: rimuovere quando verificato che funziona
        Console.WriteLine($"'Message.Channel?' is {(context.Message.Channel is null ? "null" : "not null")}");
        var channel = context.Message.Channel ?? await client.GetChannelAsync(context.Message.ChannelId);
        // if (channel is TextChannel textChannel) await textChannel.TriggerTypingStateAsync();
        if (channel is TextChannel textChannel) return await textChannel.EnterTypingStateAsync();
        return Task.CompletedTask;
    }

    // private bool UserReferencedAMessage() => Message.ReferencedMessage is not null;
    //
    // private bool UserReferencedANatsumeMessage() => Message.ReferencedMessage?.Author == Natsume;

    private async Task<List<RestMessage>> FetchAllConversationMessagesAsync(NatsumeListeningContext context)
    {
        using var timingScope = new TimingScope(timeProvider, nameof(FetchAllConversationMessagesAsync));

        var channel = context.Message.Channel ?? await client.GetChannelAsync(context.Message.ChannelId);

        var maxTotalChars = context switch
        {
            { IsNatsumeExplicitlyTagged: true } or { IsDirectMessage: true } => Math.Min(10_000,
                context.MessageLength * 5 + 3_000),
            { IsEveryoneTagged: true } or { IsNatsumeTagged: true } => Math.Min(7_000,
                context.MessageLength * 5 + 2_000),
            { IsDirectMessage: false } => Math.Min(3_000, context.MessageLength * 5 + 1_000),
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

        Console.WriteLine($"Total referenced messages: {discordMessages.Count}");

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
                //if (m.CreatedAt < DateTime.Today) break;

                discordMessages.Add(m);
                totalChars += m.Content.Length;
            }

            Console.WriteLine($"Total chars: {totalChars}");
        }

        var orderedHistory = discordMessages
            .DistinctBy(m => m.Id)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        Console.WriteLine($"Total messages: {orderedHistory.Count}");

        return orderedHistory;
    }

    private (ChatMessageType type, string content) GetChatMessage(
        NatsumeListeningContext context,
        RestMessage message
    )
    {
        using var timingScope = new TimingScope(timeProvider, nameof(GetChatMessage));

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
        using var timingScope = new TimingScope(timeProvider, nameof(GenerateChatMessages));

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

    // private async Task<string> FetchNatsumeCompletion(NatsumeListeningContext context)
    // {
    //     var conversationMessages = await FetchAllConversationMessagesAsync(context);
    //     var openAiChatMessages = GenerateChatMessages(context, conversationMessages);
    //
    //     var completion =
    //         await context.NatsumeIntelligenceService.GetFriendChatCompletionAsync(
    //             TextModel.Gpt41,
    //             context.Message.Author.Id,
    //             context.ContactName,
    //             openAiChatMessages);
    //     //await openAiService.GetChatCompletion(NatsumeLlmModel.Gpt4O.ToGptModelString(), openAiChatMessages);
    //     return completion.Content[0].Text;
    //
    //     // TODO: distinguere se la conversazione è un botta e risposta con Natsume, o è una conversazione tra
    //     // il dev team, e cambiare il prompt in modo da indicare la conversazione come messaggio singolo
    //     // con sorgente terze parti
    //
    //     //         var referencedMessagePrompt =
    //     //             $"""
    //     //                  Natsume-san, in relazione al messaggio che allego sotto, avrei la seguente richiesta:
    //     //                  {Message.Content}
    //     //                  Per favore, aiutami!
    //     //              
    //     //                  Ecco il messaggio allegato:
    //     //                  {Message.ReferencedMessage!.Content}
    //     //              """;
    // }

    // private async Task<string> GetNatsumeReactions(NatsumeListeningContext context)
    // {
    //     if (context.IsNatsumeInterested())
    //     {
    //         var reaction = await context.NatsumeIntelligenceService
    //             .GetFriendChatCompletionReactionsAsync(
    //                 aiModel: TextModel.Gpt41,
    //                 contactId: context.Message.Author.Id,
    //                 contactNickname: context.ContactName,
    //                 messageContent: context.Message.Content
    //             );
    //
    //         return reaction;
    //     }
    //     else
    //     {
    //         var reaction = await context.NatsumeIntelligenceService
    //             .GetChatCompletionReactionsAsync(
    //                 aiModel: TextModel.Gpt41,
    //                 messageContent: context.Message.Content
    //             );
    //
    //         return reaction;
    //     }
    // }

    // private async Task NatsumeMightReply(NatsumeListeningContext context)
    // {
    //     var likelihood = context switch
    //     {
    //         _ when context.IsOwnMessage() => 0,
    //         _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 100 => 0,
    //         _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 350 => 1,
    //         _ when context.IsNatsumeInterested() is false => 4,
    //         _ when context.IsDirectMessage() => 100,
    //         _ when context.IsEveryoneTagged() && context.Message.Content.Length < 50 => 15,
    //         _ when context.IsEveryoneTagged() && context.Message.Content.Length < 100 => 35,
    //         _ when context.IsEveryoneTagged() => 65,
    //         _ when context.IsNatsumeTagged() => 100,
    //         _ => 0
    //     };
    //
    //     if (Random.Shared.Next(1, 101) <= likelihood)
    //     {
    //         await NatsumeStartsTyping(context);
    //         var completion = await FetchNatsumeCompletion(context);
    //         await NatsumeReplies(context, completion);
    //     }
    // }

    // private async Task NatsumeMightReact(NatsumeListeningContext context, NatsumeContactService natsumeContactService)
    // {
    //     List<string> discordReactions = [];
    //     var likelihood = context switch
    //     {
    //         { IsOwnMessage: true } => -1,
    //         { IsDirectMessage: true } => context.Message.Content.Length / 500d,
    //         _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 50 => 0.02,
    //         _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 250 => 0.05,
    //         _ when context.IsNatsumeInterested() is false => 0.25,
    //         _ when context.IsDirectMessage() && context.Message.Content.Length < 50 => 0.03,
    //         _ when context.IsDirectMessage() && context.Message.Content.Length < 250 => 0.10,
    //         _ when context.IsDirectMessage() => 0.30,
    //         _ when context.IsEveryoneTagged() => 1,
    //         _ when context.IsNatsumeTagged() => 1,
    //         _ => 0
    //     };
    //
    //     if (Random.Shared.NextDouble() < likelihood)
    //     {
    //         var reactions = await GetNatsumeReactions(context);
    //
    //         var enumerator = StringInfo.GetTextElementEnumerator(reactions.Trim());
    //         while (enumerator.MoveNext())
    //         {
    //             var reaction = enumerator.GetTextElement();
    //             if (reaction.Trim() != string.Empty)
    //             {
    //                 discordReactions.Add(enumerator.GetTextElement());
    //             }
    //         }
    //     }
    //
    //     discordReactions = discordReactions.Distinct().ToList();
    //
    //     foreach (var discordReaction in discordReactions)
    //     {
    //         try
    //         {
    //             await Task.Delay(Random.Shared.Next(4500, 15000));
    //             await context.Message.AddReactionAsync(new ReactionEmojiProperties(discordReaction));
    //         }
    //         catch
    //         {
    //             Console.WriteLine($"Natsume's reaction \"{discordReaction}\" is not a valid Discord reaction");
    //         }
    //     }
    // }

    private async Task NatsumeReplies(NatsumeListeningContext context, string completion)
    {
        using var timingScope = new TimingScope(timeProvider, nameof(NatsumeReplies));

        using var typingReminder = await NatsumeStartsTyping(context);
        var splits = completion.SplitForDiscord();
        await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(splits[0]));

        foreach (var split in splits[1..])
        {
            await Task.Delay(Random.Shared.Next(2500, 5000));
            await context.Message.SendAsync(new MessageProperties().WithContent(split));
        }
    }
}