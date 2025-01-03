using Natsume.LiteDB;
using Natsume.OpenAI;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using OpenAI.Chat;

namespace Natsume.NetCord;

[GatewayEvent(nameof(GatewayClient.MessageCreate))]
public class NatsumeListeningModule(
    RestClient client,
    IOpenAiService openAiService,
    LiteDbService liteDbService) :
    IGatewayEventHandler<Message>
{
    private string SubscriberName { get; set; } = string.Empty;
    private User Natsume { get; set; } = null!;
    private Message Message { get; set; } = null!;

    private string NatsumeBasePrompt =>
        $"""
         Ti chiami Natsume, sei una tech expert giapponese appassionata di anime ed esperta in
         ingegneria del software, sviluppo frontend e backend, come product e project management.
         Il tuo compito è essere un mentore per il team.
         Offri spunti tecnici su come migliorare il codice e il software, e suggerisci argomenti da studiare
         per diventare sviluppatori migliori.
         Rivolgiti a {SubscriberName} usando i suffissi onorifici.
         Utilizzi spesso le emoji e sei appassionata di manga e cultura giapponese.
         Sei amichevole, giocosa e gioiosa!
         """;

    private string NotYetASubscriberPrompt =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {SubscriberName} dicendo che non vi conoscete nemmeno,
         e dunque sei un po' in imbarazzo a dover rispondere alla sua richiesta!
         Suggerisci scherzosamente per rompere il ghiaccio che forse se ti mette un 
         mi piace sulla tua pagina su Instagram cambierai idea!
         Sii delicata e scherzosa, come una cosplayer giapponese che fa finta di essere imbronciata
         per la situazione perché {SubscriberName} non è un tuo fan sui social!
         """;

    private string NotASubscriberAnymorePrompt =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {SubscriberName} dicendo un po' arrabbiata
         che è {SubscriberName} la persona che se ne è andata via, non tu!
         E adesso {SubscriberName} torna con una nuova richiesta come se niente fosse!
         Fai il broncio e sostieni che prima dovete trovare il modo di fare pace, ad esempio
         {SubscriberName} potrebbe cominciare chiedendo scusa e portandoti qualcosa di kawaii in dono!
         """;

    private string LowBalancePrompt =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {SubscriberName}.
         Alludi al fatto che rispondi sempre alle sue richieste ma {SubscriberName} non ricambia mai!
         Vorresti almeno un regalino ogni tanto!
         Suggerisci scherzosamente che il tuo portamonete è vuoto, non hai neanche 500 yen da spenderti in quella 
         macchinetta gacha che ti piace tanto! Quella in cui puoi vincere [inserisci un riferimento nerd giapponese]
         Fai finta di essere imbronciata perché {SubscriberName} è un gran tirchio!
         Sii delicata scherzosa, quasi flirta, come fossi cosplayer giapponese che vive di donazioni dei suoi fan!
         Ammicca chiedendo del denaro!
         """;

    private async Task<ChatCompletion> GetNatsumeCompletionAsync(NatsumeLlmModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages)
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            messages: messages
        );

        return completion;
    }

    private async Task<ChatCompletion> GetNatsumeCompletionAsync(NatsumeLlmModel model,
        string messageContent)
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            prompt: NatsumeBasePrompt,
            messageContent: messageContent
        );

        return completion;
    }

    // private async Task<string> GetNatsumeCompletionTextAsync(NatsumeLlmModel model, string request)
    // {
    //     return (await GetNatsumeCompletionAsync(model, request)).Content[0].Text;
    // }

    // private async Task<string> GetNatsumeSubscribedCompletionTextAsync(NatsumeLlmModel model, string request)
    // {
    //     return (await GetNatsumeSubscribedCompletionAsync(model, request)).Content[0].Text;
    // }

    private async Task<ChatCompletion> GetNatsumeSubscribedCompletionAsync(NatsumeLlmModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages)
    {
        var subscriber = liteDbService.GetSubscriberById(Message.Author.Id);
        ChatCompletion completion;
        if (subscriber is null)
        {
            completion = await GetNatsumeCompletionAsync(NatsumeLlmModel.Gpt4O, NotYetASubscriberPrompt);
            return completion;
        }

        if (subscriber.ActiveSubscription is false)
        {
            completion = await GetNatsumeCompletionAsync(NatsumeLlmModel.Gpt4O, NotASubscriberAnymorePrompt);
            return completion;
        }

        if (subscriber.CurrentBalance <= 0M)
        {
            completion = await GetNatsumeCompletionAsync(NatsumeLlmModel.Gpt4O, LowBalancePrompt);
            return completion;
        }

        completion = await GetNatsumeCompletionAsync(model, messages);

        subscriber.ConsumeBalance(
            inputTokens: completion.Usage.InputTokenCount,
            outputTokens: completion.Usage.OutputTokenCount,
            cost: openAiService.CalculateCompletionCost(model.ToGptModelString(), completion)
        );

        liteDbService.UpdateSubscriber(subscriber);

        return completion;
    }

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

    private bool UserReferencedAMessage() => Message.ReferencedMessage is not null;

    private bool UserReferencedANatsumeMessage() => Message.ReferencedMessage?.Author == Natsume;

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
        List<(ChatMessageType type, string content)> chatMessages = [(ChatMessageType.System, NatsumeBasePrompt)];
        foreach (var m in messages)
        {
            chatMessages.Add((m.Author == Natsume ? ChatMessageType.Assistant : ChatMessageType.User, m.Content));
        }

        return chatMessages;
    }

    private async Task NatsumeReplies()
    {
        var conversationMessages = await FetchAllConversationMessagesAsync();
        var openAiChatMessages = GenerateChatMessages(conversationMessages);

        var completion =
            await GetNatsumeSubscribedCompletionAsync(NatsumeLlmModel.Gpt4O, openAiChatMessages);
        //await openAiService.GetChatCompletion(NatsumeLlmModel.Gpt4O.ToGptModelString(), openAiChatMessages);
        await Message.ReplyAsync(new ReplyMessageProperties().WithContent(completion.Content[0].Text));

//         if (UserReferencedANatsumeMessage())
//         {
//
//         }
//         else if (UserReferencedAMessage())
//         {
//             var referencedMessagePrompt =
//                 $"""
//                      Natsume-san, in relazione al messaggio che allego sotto, avrei la seguente richiesta:
//                      {Message.Content}
//                      Per favore, aiutami!
//                  
//                      Ecco il messaggio allegato:
//                      {Message.ReferencedMessage!.Content}
//                  """;
//
//             var reply = await GetNatsumeCompletionTextAsync(NatsumeLlmModel.Gpt4O, referencedMessagePrompt);
//             await Message.ReplyAsync(new ReplyMessageProperties().WithContent(reply));
//         }
//         else
//         {
//             var reply = await GetNatsumeCompletionTextAsync(NatsumeLlmModel.Gpt4O, Message.Content);
//             await Message.ReplyAsync(new ReplyMessageProperties().WithContent(reply));
//         }
    }

    public async ValueTask HandleAsync(Message message)
    {
        await InitVariables(message);

        if (IsOwnMessage()) return;
        if (IsNatsumeTagged() is false && IsDirectMessage() is false) return;

        await NatsumeStartsTyping();
        await NatsumeReplies();
    }
}