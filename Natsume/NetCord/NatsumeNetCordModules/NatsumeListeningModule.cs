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
    private string ContactName { get; set; } = string.Empty;
    private User Natsume { get; set; } = null!;
    private Message Message { get; set; } = null!;

    private bool IsOwnMessage() => Message.Author == Natsume;

    private bool IsNatsumeTagged() => Message.MentionedUsers.Contains(Natsume) || Message.MentionEveryone;

    private bool IsDirectMessage() => Message.Channel is DMChannel;

    private async Task InitVariables(Message message)
    {
        Message = message;
        ContactName = Message.Author.GetName();
        Natsume = await client.GetCurrentUserAsync();
    }

    private async Task NatsumeStartsTyping()
    {
        if (Message.Channel is not null)
        {
            await Message.Channel.TriggerTypingStateAsync();
            return;
        }

        var channel = await client.GetChannelAsync(Message.ChannelId);
        if (channel is TextChannel textChannel)
            await textChannel.TriggerTypingStateAsync();
        else if (channel is DMChannel dmChannel)
            await dmChannel.TriggerTypingStateAsync();
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

    private (ChatMessageType type, string content) GetChatMessage(RestMessage message)
    {
        var messageContent = message.Content;
        foreach (var user in message.MentionedUsers)
        {
            messageContent = message.Content.Replace($"<@{user.Id}>", user.GetName());
        }

        if (message.Author != Natsume && message.Author != Message.Author)
        {
            messageContent = $"{message.Author.GetName()} dice:\n {messageContent}";
        }

        return message.Author switch
        {
            _ when message.Author == Natsume => (ChatMessageType.Assistant, messageContent),
            _ => (ChatMessageType.User, messageContent)
        };
    }

    private List<(ChatMessageType type, string content)> GenerateChatMessages(List<RestMessage> messages)
    {
        List<(ChatMessageType type, string content)> chatMessages =
        [
            (ChatMessageType.System, NatsumeAi.SystemPrompt(ContactName))
        ];

        chatMessages.AddRange(messages.Select(GetChatMessage));

        return chatMessages;
    }

    private async Task<string> FetchNatsumeCompletion()
    {
        var conversationMessages = await FetchAllConversationMessagesAsync();
        var openAiChatMessages = GenerateChatMessages(conversationMessages);

        var completion =
            await natsumeAi.GetFriendCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                Message.Author.Id,
                ContactName,
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