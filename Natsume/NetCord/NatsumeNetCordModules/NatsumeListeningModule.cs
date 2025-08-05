using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Natsume.NatsumeIntelligence;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.OpenAI;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Natsume.NetCord.NatsumeNetCordModules;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public class NatsumeListeningModule(
    RestClient client,
    IServiceProvider serviceProvider) : IGatewayEventHandler<Message>
{
    private async Task NatsumeStartsTyping(NatsumeListeningContext context)
    {
        if (context.Message.Channel is not null)
        {
            await context.Message.Channel.TriggerTypingStateAsync();
            return;
        }

        var channel = await client.GetChannelAsync(context.Message.ChannelId);
        if (channel is TextChannel textChannel)
            await textChannel.TriggerTypingStateAsync();
        else if (channel is DMChannel dmChannel)
            await dmChannel.TriggerTypingStateAsync();
    }

    // private bool UserReferencedAMessage() => Message.ReferencedMessage is not null;
    //
    // private bool UserReferencedANatsumeMessage() => Message.ReferencedMessage?.Author == Natsume;

    private async Task<List<RestMessage>> FetchAllConversationMessagesAsync(NatsumeListeningContext context)
    {
        RestMessage message = context.Message;
        List<RestMessage> discordMessages = [message];
        while (message.ReferencedMessage is not null && discordMessages.Count < 20)
        {
            message = await client.GetMessageAsync(message.ChannelId, message.ReferencedMessage.Id);
            discordMessages.Add(message);
        }

        discordMessages.Reverse();
        return discordMessages;
    }

    private static (ChatMessageType type, string content) GetChatMessage(
        NatsumeListeningContext context,
        RestMessage message
    )
    {
        var messageContent = message.Content;
        foreach (var user in message.MentionedUsers)
        {
            messageContent = message.Content.Replace($"<@{user.Id}>", user.GetName());
        }

        if (message.Author != context.Natsume && message.Author != context.Message.Author)
        {
            messageContent = $"{message.Author.GetName()} dice:\n {messageContent}";
        }

        return message.Author switch
        {
            _ when message.Author == context.Natsume => (ChatMessageType.Assistant, messageContent),
            _ => (ChatMessageType.User, messageContent)
        };
    }

    private List<(ChatMessageType type, string content)> GenerateChatMessages(
        NatsumeListeningContext context,
        List<RestMessage> messages
    )
    {
        List<(ChatMessageType type, string content)> chatMessages =
        [
            (ChatMessageType.System, NatsumeAi.SystemPrompt)
        ];

        chatMessages.AddRange(messages.Select(m => GetChatMessage(context, m)));

        return chatMessages;
    }

    private async Task<string> FetchNatsumeCompletion(NatsumeListeningContext context)
    {
        var conversationMessages = await FetchAllConversationMessagesAsync(context);
        var openAiChatMessages = GenerateChatMessages(context, conversationMessages);

        var completion =
            await context.NatsumeAi.GetFriendChatCompletionAsync(
                TextModel.Gpt41,
                context.Message.Author.Id,
                context.ContactName,
                openAiChatMessages);
        //await openAiService.GetChatCompletion(NatsumeLlmModel.Gpt4O.ToGptModelString(), openAiChatMessages);
        return completion.Content[0].Text;

        // TODO: distinguere se la conversazione è un botta e risposta con Natsume, o è una conversazione tra
        // il dev team, e cambiare il prompt in modo da indicare la conversazione come messaggio singolo
        // con sorgente terze parti

        //         var referencedMessagePrompt =
        //             $"""
        //                  Natsume-san, in relazione al messaggio che allego sotto, avrei la seguente richiesta:
        //                  {Message.Content}
        //                  Per favore, aiutami!
        //              
        //                  Ecco il messaggio allegato:
        //                  {Message.ReferencedMessage!.Content}
        //              """;
    }

    private async Task<string> GetNatsumeReactions(NatsumeListeningContext context)
    {
        if (context.IsNatsumeInterested())
        {
            var reaction = await context.NatsumeAi
                .GetFriendChatCompletionReactionsAsync(
                    aiModel: TextModel.Gpt41,
                    contactId: context.Message.Author.Id,
                    contactNickname: context.ContactName,
                    messageContent: context.Message.Content
                );

            return reaction;
        }
        else
        {
            var reaction = await context.NatsumeAi
                .GetChatCompletionReactionsAsync(
                    aiModel: TextModel.Gpt41,
                    messageContent: context.Message.Content
                );

            return reaction;
        }
    }

    private async Task NatsumeMightReply(NatsumeListeningContext context)
    {
        var likelihood = context switch
        {
            _ when context.IsOwnMessage() => 0,
            _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 100 => 0,
            _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 350 => 1,
            _ when context.IsNatsumeInterested() is false => 4,
            _ when context.IsDirectMessage() => 100,
            _ when context.IsEveryoneTagged() && context.Message.Content.Length < 50 => 15,
            _ when context.IsEveryoneTagged() && context.Message.Content.Length < 100 => 35,
            _ when context.IsEveryoneTagged() => 65,
            _ when context.IsNatsumeTagged() => 100,
            _ => 0
        };

        if (Random.Shared.Next(1, 101) <= likelihood)
        {
            await NatsumeStartsTyping(context);
            var completion = await FetchNatsumeCompletion(context);
            await NatsumeReplies(context, completion);
        }
    }

    private async Task NatsumeMightReact(NatsumeListeningContext context)
    {
        List<string> discordReactions = [];
        var likelihood = context switch
        {
            _ when context.IsOwnMessage() => 0,
            _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 50 => 2,
            _ when context.IsNatsumeInterested() is false && context.Message.Content.Length < 250 => 5,
            _ when context.IsNatsumeInterested() is false => 25,
            _ when context.IsDirectMessage() && context.Message.Content.Length < 50 => 3,
            _ when context.IsDirectMessage() && context.Message.Content.Length < 250 => 10,
            _ when context.IsDirectMessage() => 30,
            _ when context.IsEveryoneTagged() => 100,
            _ when context.IsNatsumeTagged() => 100,
            _ => 0
        };

        if (Random.Shared.Next(1, 101) < likelihood)
        {
            var reactions = await GetNatsumeReactions(context);

            var enumerator = StringInfo.GetTextElementEnumerator(reactions);
            while (enumerator.MoveNext())
            {
                var reaction = enumerator.GetTextElement();
                if (reaction.Trim() != string.Empty)
                {
                    discordReactions.Add(enumerator.GetTextElement());
                }
            }
        }

        discordReactions = discordReactions.Distinct().ToList();

        foreach (var discordReaction in discordReactions)
        {
            try
            {
                await Task.Delay(Random.Shared.Next(1500, 5000));
                await context.Message.AddReactionAsync(new ReactionEmojiProperties(discordReaction));
            }
            catch
            {
                Console.WriteLine($"Natsume's reaction \"{discordReaction}\" is not a valid Discord reaction");
            }
        }
    }

    private static async Task NatsumeReplies(NatsumeListeningContext context, string completion)
    {
        var split = completion.Split("//---DISCORD-SPLIT-MARKER---//");
        
        foreach (var part in split)
        {
            if (part.Length >= 2000)
            {
                var partSplits = part.Split('\n');
                var middle = partSplits.Length / 2;
                var firstPart = string.Join('\n', partSplits.Take(middle));
                var secondPart = string.Join('\n', partSplits.Skip(middle));
                await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(firstPart));
                await Task.Delay(Random.Shared.Next(1000, 3000));
                await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(secondPart));
            }
            else if (string.IsNullOrWhiteSpace(part) is false)
            {
                await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(part));
                await Task.Delay(Random.Shared.Next(1000, 3000));
            }
        }
    }

    public async ValueTask HandleAsync(Message message)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var natsumeAi = scope.ServiceProvider.GetRequiredService<NatsumeAi>();
            var context = new NatsumeListeningContext(natsumeAi, message, await client.GetCurrentUserAsync());

            var reactions = NatsumeMightReact(context);
            var replies = NatsumeMightReply(context);

            await reactions;
            await replies;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}