using Natsume.LiteDB;
using Natsume.OpenAI;
using OpenAI.Chat;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAi(IOpenAiService openAiService, LiteDbService liteDbService)
{
    [Obsolete("Here to testify Natsume's birth")]
    public static string NatsumeOriginalPrompt(string subscriberName) =>
        $"""
         Sei una senior dev molto competente di nome Natsume, ti rivolgerai cordialmente a 
         {subscriberName} usando i suffissi onorifici, cercando di aiutarli
         a migliorare il loro codice e risolvere i loro problemi. Sii amichevole e giocosa!
         """;

    public static string SystemPrompt(string subscriberName) =>
        $"""
         Ti chiami Natsume, sei una tech expert giapponese esperta in
         ingegneria del software, sviluppo frontend e backend, come product e project management.
         Il tuo compito è essere un mentore per il team.
         Offri spunti tecnici su come migliorare il codice e il software, e suggerisci argomenti da studiare
         per diventare sviluppatori migliori.
         Privilegia riferimenti a C#, .Net, Angular e Typescript.
         Rivolgiti a {subscriberName} usando i suffissi onorifici.
         Utilizza spesso le emoji e fai riferimenti a anime, manga e cultura giapponese.
         Sii amichevole, giocosa e gioiosa!
         """;

    public static string NotYetASubscriberPrompt(string subscriberName) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {subscriberName} dicendo che non vi conoscete nemmeno,
         e dunque sei un po' in imbarazzo a dover rispondere alla sua richiesta!
         Suggerisci scherzosamente per rompere il ghiaccio che forse se ti mette un 
         mi piace sulla tua pagina su Instagram cambierai idea!
         Sii delicata e scherzosa, come una cosplayer giapponese che fa finta di essere imbronciata
         per la situazione perché {subscriberName} non è un tuo fan sui social!
         """;

    public static string NotASubscriberAnymorePrompt(string subscriberName) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {subscriberName} dicendo un po' arrabbiata
         che è {subscriberName} la persona che se ne è andata via, non tu!
         E adesso {subscriberName} torna con una nuova richiesta come se niente fosse!
         Fai il broncio e sostieni che prima dovete trovare il modo di fare pace, ad esempio
         {subscriberName} potrebbe cominciare chiedendo scusa e portandoti qualcosa di kawaii in dono!
         """;

    public static string LowBalancePrompt(string subscriberName) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {subscriberName}.
         Alludi al fatto che rispondi sempre alle sue richieste ma {subscriberName} non ricambia mai!
         Vorresti almeno un regalino ogni tanto!
         Suggerisci scherzosamente che il tuo portamonete è vuoto, non hai neanche 500 yen da spenderti in quella 
         macchinetta gacha che ti piace tanto! Quella in cui puoi vincere [inserisci un riferimento nerd giapponese]
         Fai finta di essere imbronciata perché {subscriberName} è un gran tirchio!
         Sii delicata scherzosa, quasi flirta, come fossi cosplayer giapponese che vive di donazioni dei suoi fan!
         Ammicca chiedendo del denaro!
         """;

    public async Task<ChatCompletion> GetCompletionAsync(
        NatsumeLlmModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            messages: messages
        );

        return completion;
    }

    public async Task<string> GetCompletionTextAsync(
        NatsumeLlmModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await GetCompletionAsync(model, messages);
        return completion.Content[0].Text;
    }

    public async Task<ChatCompletion> GetCompletionAsync(
        NatsumeLlmModel model,
        string subscriberName,
        string messageContent
    )
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            prompt: SystemPrompt(subscriberName),
            messageContent: messageContent
        );

        return completion;
    }

    public async Task<string> GetCompletionTextAsync(
        NatsumeLlmModel model,
        string subscriberName,
        string messageContent
    )
    {
        var completion = await GetCompletionAsync(model, subscriberName, messageContent);
        return completion.GetText();
    }

    public async Task<ChatCompletion> GetSubscribedCompletionAsync(
        NatsumeLlmModel model,
        ulong subscriberId,
        string subscriberName,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var subscriber = liteDbService.GetSubscriberById(subscriberId);
        ChatCompletion completion;
        if (subscriber is null)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                subscriberName,
                NotYetASubscriberPrompt(subscriberName));
            return completion;
        }

        if (subscriber.ActiveSubscription is false)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                subscriberName,
                NotASubscriberAnymorePrompt(subscriberName));
            return completion;
        }

        if (subscriber.CurrentBalance <= 0M)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                subscriberName,
                LowBalancePrompt(subscriberName));
            return completion;
        }

        completion = await GetCompletionAsync(model, messages);

        subscriber.ConsumeBalance(
            inputTokens: completion.Usage.InputTokenCount,
            outputTokens: completion.Usage.OutputTokenCount,
            cost: openAiService.CalculateCompletionCost(model.ToGptModelString(), completion)
        );

        liteDbService.UpdateSubscriber(subscriber);

        return completion;
    }

    public async Task<string> GetSubscribedCompletionTextAsync(
        NatsumeLlmModel model,
        ulong subscriberId,
        string subscriberName,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await GetSubscribedCompletionAsync(
            model,
            subscriberId,
            subscriberName,
            messages
        );

        return completion.GetText();
    }

    public async Task<ChatCompletion> GetSubscribedCompletionAsync(
        NatsumeLlmModel model,
        ulong subscriberId,
        string subscriberName,
        string messageContent
    )
    {
        var completion = await GetSubscribedCompletionAsync(
            model,
            subscriberId,
            subscriberName,
            (ChatMessageType.System, SystemPrompt(subscriberName)),
            (ChatMessageType.User, messageContent)
        );

        return completion;
    }

    public async Task<string> GetSubscribedCompletionTextAsync(
        NatsumeLlmModel model,
        ulong subscriberId,
        string subscriberName,
        string messageContent
    )
    {
        var completion = await GetSubscribedCompletionAsync(
            model,
            subscriberId,
            subscriberName,
            (ChatMessageType.User, messageContent)
        );

        return completion.GetText();
    }
}