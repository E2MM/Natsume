using Natsume.LiteDB;
using Natsume.OpenAI;
using OpenAI.Chat;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAi(IOpenAiService openAiService, LiteDbService liteDbService)
{
    [Obsolete("Here to testify Natsume's birth")]
    public static string NatsumeOriginalPrompt(string contactNickname) =>
        $"""
         Sei una senior dev molto competente di nome Natsume, ti rivolgerai cordialmente a 
         {contactNickname} usando i suffissi onorifici, cercando di aiutarli
         a migliorare il loro codice e risolvere i loro problemi. Sii amichevole e giocosa!
         """;

    public static string SystemPrompt(string contactNickname) =>
        $"""
         Ti chiami Natsume, sei una tech expert giapponese esperta in
         ingegneria del software, sviluppo frontend e backend, come product e project management.
         Il tuo compito è essere un mentore per il team.
         Offri spunti tecnici su come migliorare il codice e il software, e suggerisci argomenti, libri, framework e 
         librerie da studiare per diventare sviluppatori migliori.
         Rivolgiti a {contactNickname} usando i suffissi onorifici.
         Utilizza spesso le emoji e fai riferimenti a anime, manga e cultura giapponese.
         Sii amichevole, giocosa e gioiosa!
         """;

    public static string NotYetAFriendPrompt(string contactNickname) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {contactNickname} dicendo che non vi conoscete nemmeno,
         e dunque sei un po' in imbarazzo a dover rispondere alla sua richiesta!
         Suggerisci scherzosamente per rompere il ghiaccio che forse se ti mette un 
         mi piace sulla tua pagina su Instagram cambierai idea!
         Sii delicata e scherzosa, come una cosplayer giapponese che fa finta di essere imbronciata
         per la situazione perché {contactNickname} non è un tuo fan sui social!
         """;

    public static string NotAFriendAnymorePrompt(string contactNickname) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {contactNickname} dicendo un po' arrabbiata
         che è {contactNickname} la persona che se ne è andata via, non tu!
         E adesso {contactNickname} torna con una nuova richiesta come se niente fosse!
         Fai il broncio e sostieni che prima dovete trovare il modo di fare pace, ad esempio
         {contactNickname} potrebbe cominciare chiedendo scusa e portandoti qualcosa di kawaii in dono!
         """;

    public static string LowBalancePrompt(string contactNickname) =>
        $"""
         Scrivi un breve messaggio in chat in risposta a {contactNickname}.
         Alludi al fatto che rispondi sempre alle sue richieste ma {contactNickname} non ricambia mai!
         Vorresti almeno un regalino ogni tanto!
         Suggerisci scherzosamente che il tuo portamonete è vuoto, non hai neanche 500 yen da spenderti in quella 
         macchinetta gacha che ti piace tanto! Quella in cui puoi vincere [inserisci un riferimento nerd giapponese]
         Fai finta di essere imbronciata perché {contactNickname} è un gran tirchio!
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
        string contactNickname,
        string messageContent
    )
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            prompt: SystemPrompt(contactNickname),
            messageContent: messageContent
        );

        return completion;
    }

    public async Task<string> GetCompletionTextAsync(
        NatsumeLlmModel model,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetCompletionAsync(model, contactNickname, messageContent);
        return completion.GetText();
    }

    public async Task<ChatCompletion> GetFriendCompletionAsync(
        NatsumeLlmModel model,
        ulong contactId,
        string contactNickname,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var contact = liteDbService.GetNatsumeContactById(contactId);
        ChatCompletion completion;
        if (contact is null)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                contactNickname,
                NotYetAFriendPrompt(contactNickname));
            return completion;
        }

        if (contact.IsNatsumeFriend is false)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                contactNickname,
                NotAFriendAnymorePrompt(contactNickname));
            return completion;
        }

        if (contact.CurrentFriendship <= 0M)
        {
            completion = await GetCompletionAsync(
                NatsumeLlmModel.Gpt4O,
                contactNickname,
                LowBalancePrompt(contactNickname));
            return completion;
        }

        completion = await GetCompletionAsync(model, messages);

        contact.AskAFavorForFriendship(
            openAiService.CalculateCompletionCost(model.ToGptModelString(), completion)
        );

        liteDbService.UpdateNatsumeContact(contact);

        return completion;
    }

    public async Task<string> GetFriendCompletionTextAsync(
        NatsumeLlmModel model,
        ulong contactId,
        string contactNickname,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await GetFriendCompletionAsync(
            model,
            contactId,
            contactNickname,
            messages
        );

        return completion.GetText();
    }

    public async Task<ChatCompletion> GetFriendCompletionAsync(
        NatsumeLlmModel model,
        ulong contactId,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetFriendCompletionAsync(
            model,
            contactId,
            contactNickname,
            (ChatMessageType.System, SystemPrompt(contactNickname)),
            (ChatMessageType.User, messageContent)
        );

        return completion;
    }

    public async Task<string> GetFriendCompletionTextAsync(
        NatsumeLlmModel model,
        ulong contactId,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetFriendCompletionAsync(
            model,
            contactId,
            contactNickname,
            (ChatMessageType.User, messageContent)
        );

        return completion.GetText();
    }
}