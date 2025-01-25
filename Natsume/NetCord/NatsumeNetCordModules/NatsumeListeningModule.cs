using Natsume.NetCord.NatsumeAI;
using Natsume.OpenAI;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Natsume.NetCord.NatsumeNetCordModules;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public class NatsumeListeningModule(
    RestClient client,
    NatsumeAi natsumeAi) :
    IGatewayEventHandler<Message>
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
            (ChatMessageType.System, NatsumeAi.SystemPrompt(context.ContactName))
        ];

        chatMessages.AddRange(messages.Select(m => GetChatMessage(context, m)));

        return chatMessages;
    }

    private async Task<string> FetchNatsumeCompletion(NatsumeListeningContext context)
    {
        var conversationMessages = await FetchAllConversationMessagesAsync(context);
        var openAiChatMessages = GenerateChatMessages(context, conversationMessages);

        var completion =
            await natsumeAi.GetFriendChatCompletionAsync(
                NatsumeChatModel.Gpt4O,
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

    private async Task NatsumeMightReact(NatsumeListeningContext context)
    {
        //if (Random.Shared.NextDouble() > 0.69) return;

        var reaction = await natsumeAi
            .GetFriendChatCompletionReactionAsync(
                model: NatsumeChatModel.Gpt4O,
                contactId: context.Message.Author.Id,
                contactNickname: context.ContactName,
                messageContent: context.Message.Content
            );

        try
        {
            await context.Message.AddReactionAsync(new ReactionEmojiProperties(reaction));
        }
        catch
        {
            Console.WriteLine($"Natsume's reaction \"{reaction}\" is not a valid Discord reaction");
        }
    }

    private static async Task NatsumeReplies(NatsumeListeningContext context, string completion)
    {
        await context.Message.ReplyAsync(new ReplyMessageProperties().WithContent(completion));
    }

    public async ValueTask HandleAsync(Message message)
    {
        try
        {
            var context = new NatsumeListeningContext(message, await client.GetCurrentUserAsync());

            if (context.IsOwnMessage()) return;
            if (context.IsNatsumeTagged() is false && context.IsDirectMessage() is false) return;

            await NatsumeStartsTyping(context);
            await NatsumeMightReact(context);
            var completion = await FetchNatsumeCompletion(context);
            await NatsumeReplies(context, completion);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}