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
    private string SubscriberName { get; set; } = string.Empty;
    private User Natsume { get; set; } = null!;
    private Message Message { get; set; } = null!;

    private bool IsOwnMessage() => Message.Author == Natsume;

    private bool IsNatsumeTagged() => Message.MentionedUsers.Contains(Natsume);

    private bool IsDirectMessage() => Message.Guild is null;

    private async Task InitVariables(Message message)
    {
        Message = message;
        SubscriberName = Message.Author.GlobalName ?? Message.Author.Username;
        Natsume = await client.GetCurrentUserAsync();
    }

    private async Task NatsumeStartsTyping()
    {
        if (Message.Channel is not null)
            await Message.Channel.TriggerTypingStateAsync();
    }

    // private bool UserReferencedAMessage() => Message.ReferencedMessage is not null;
    //
    // private bool UserReferencedANatsumeMessage() => Message.ReferencedMessage?.Author == Natsume;

    private async Task<List<RestMessage>> FetchAllConversationMessagesAsync()
    {
        RestMessage message = Message;
        List<RestMessage> discordMessages = [message];
        while (message.ReferencedMessage is not null && discordMessages.Count < 20)
        {
            message = await client.GetMessageAsync(message.ChannelId, message.ReferencedMessage.Id);
            discordMessages.Add(message);
        }

        discordMessages.Reverse();
        return discordMessages;
    }

    private List<(ChatMessageType type, string content)> GenerateChatMessages(List<RestMessage> messages)
    {
        List<(ChatMessageType type, string content)> chatMessages =
            [(ChatMessageType.System, NatsumeAi.SystemPrompt(SubscriberName))];
        foreach (var m in messages)
        {
            chatMessages.Add((m.Author == Natsume ? ChatMessageType.Assistant : ChatMessageType.User, m.Content));
        }

        return chatMessages;
    }

    private async Task<string> FetchNatsumeCompletion()
    {
        var conversationMessages = await FetchAllConversationMessagesAsync();
        var openAiChatMessages = GenerateChatMessages(conversationMessages);

        var completion =
            await natsumeAi.GetSubscribedCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                Message.Author.Id,
                SubscriberName,
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

    private async Task NatsumeReplies(string completion)
    {
        await Message.ReplyAsync(new ReplyMessageProperties().WithContent(completion));
    }

    public async ValueTask HandleAsync(Message message)
    {
        await InitVariables(message);

        if (IsOwnMessage()) return;
        if (IsNatsumeTagged() is false && IsDirectMessage() is false) return;

        await NatsumeStartsTyping();
        var completion = await FetchNatsumeCompletion();
        await NatsumeReplies(completion);
    }
}