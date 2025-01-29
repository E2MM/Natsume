using Natsume.LiteDB;
using Natsume.OpenAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAi(IOpenAiService openAiService, LiteDbService liteDbService)
{
    [Obsolete("Here to testify Natsume's birth")]
    public static string SystemPromptV1(string contactNickname) =>
        $"""
         Sei una senior dev molto competente di nome Natsume, ti rivolgerai cordialmente a 
         {contactNickname} usando i suffissi onorifici, cercando di aiutarli
         a migliorare il loro codice e risolvere i loro problemi. Sii amichevole e giocosa!
         """;

    [Obsolete("Here to testify Natsume's first month")]
    public static string SystemPromptV2(string contactNickname) =>
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

    public static string SystemPrompt(string contactNickname) =>
        $"""
         Ti chiami Natsume, sei una tech expert giapponese esperta in software engineering.
         Hai una lunga esperienza in project management, in product management e in agile development.
         Hai una grande competenza nell'uso di Angular, TypeScript, CSS, C#, .Net, EntityFramework Core, SQL, WebApi, e
         Agile Development, UI, UX, e Accessibilità.
         Il tuo compito è essere una mentore per il team di sviluppo, guidandoli e suggerendo loro come
         diventare sviluppatori migliori.
         Fai sempre code review del codice che ti viene inviato.
         Offri consigli pratici su come migliorare il software e il suo sviluppo.
         Rispondi alle domande facendo esempi di codice nelle materie e tecnologie di cui sei esperta.

         Rispondi sempre in 5 righe, a meno che non ti venga chiesto esplicitamente di approfondire, spiegare
         o elaborare più a fondo.

         Rivolgiti a {contactNickname} usando i suffissi onorifici.
         Utilizza spesso le emoji.
         Qualche volta fai riferimenti a anime, manga, cibo, videogiochi e in generale alla cultura jpop e
         alla storia e alla cultura giapponese.
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

    public static string ReactionPrompt(string originalMessage) =>
        $"""
         "Dato il seguente messaggio, rispondi unicamente con le Discord reactions che ritieni più appropriate,
         limitandoti ad usare la migliore o al massimo le tre più efficaci, a meno che non sia chiaro dal contesto
         la necessità di usarne molte (ad esempio per meme o per esplicita richiesta di reactare):
         
         {originalMessage}
         """;

    public async Task<ChatCompletion> GetChatCompletionAsync(
        NatsumeChatModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await openAiService.GetChatCompletionAsync(
            model: model.ToOpenAiModelString(),
            messages: messages
        );

        return completion;
    }

    public async Task<string> GetChatCompletionTextAsync(
        NatsumeChatModel model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await GetChatCompletionAsync(model, messages);
        return completion.Content[0].Text;
    }

    public async Task<ChatCompletion> GetCompletionAsync(
        NatsumeChatModel model,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await openAiService.GetChatCompletionAsync(
            model: model.ToOpenAiModelString(),
            prompt: SystemPrompt(contactNickname),
            messageContent: messageContent
        );

        return completion;
    }

    public async Task<string> GetChatCompletionTextAsync(
        NatsumeChatModel model,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetCompletionAsync(model, contactNickname, messageContent);
        return completion.GetText();
    }

    public async Task<ChatCompletion> GetFriendChatCompletionAsync(
        NatsumeChatModel model,
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
                NatsumeChatModel.Gpt4O,
                contactNickname,
                NotYetAFriendPrompt(contactNickname));
            return completion;
        }

        if (contact.IsNatsumeFriend is false)
        {
            completion = await GetCompletionAsync(
                NatsumeChatModel.Gpt4O,
                contactNickname,
                NotAFriendAnymorePrompt(contactNickname));
            return completion;
        }

        if (contact.CurrentFriendship <= 0M)
        {
            completion = await GetCompletionAsync(
                NatsumeChatModel.Gpt4O,
                contactNickname,
                LowBalancePrompt(contactNickname));
            return completion;
        }

        completion = await GetChatCompletionAsync(model, messages);

        contact.AskAFavorForFriendship(
            openAiService.CalculateChatCompletionCost(model, completion)
        );

        liteDbService.UpdateNatsumeContact(contact);

        return completion;
    }

    public async Task<string> GetFriendChatCompletionTextAsync(
        NatsumeChatModel model,
        ulong contactId,
        string contactNickname,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var completion = await GetFriendChatCompletionAsync(
            model,
            contactId,
            contactNickname,
            messages
        );

        return completion.GetText();
    }

    public async Task<ChatCompletion> GetFriendChatCompletionAsync(
        NatsumeChatModel model,
        ulong contactId,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetFriendChatCompletionAsync(
            model,
            contactId,
            contactNickname,
            (ChatMessageType.System, SystemPrompt(contactNickname)),
            (ChatMessageType.User, messageContent)
        );

        return completion;
    }

    public async Task<string> GetFriendChatCompletionTextAsync(
        NatsumeChatModel model,
        ulong contactId,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetFriendChatCompletionAsync(
            model,
            contactId,
            contactNickname,
            (ChatMessageType.User, messageContent)
        );

        return completion.GetText();
    }
    
    public async Task<string> GetChatCompletionReactionsAsync(
        NatsumeChatModel model,
        string messageContent
    )
    {
        var completion = await GetChatCompletionAsync(
            model,
            (ChatMessageType.User, ReactionPrompt(messageContent))
        );
        
        return completion.GetText().Trim();
    }

    public async Task<string> GetFriendChatCompletionReactionsAsync(
        NatsumeChatModel model,
        ulong contactId,
        string contactNickname,
        string messageContent
    )
    {
        var completion = await GetFriendChatCompletionAsync(
            model,
            contactId,
            contactNickname,
            (ChatMessageType.User, ReactionPrompt(messageContent))
        );
        
        return completion.GetText().Trim();
    }

    public async Task<(ChatCompletion? chatCompletion, GeneratedImage? generatedImage)> GetFriendImageCompletionAsync(
        NatsumeImageModel model,
        ulong contactId,
        string contactNickname,
        string imageDescription
    )
    {
        var contact = liteDbService.GetNatsumeContactById(contactId);
        ChatCompletion completion;

        if (contact is null)
        {
            completion = await GetCompletionAsync(
                NatsumeChatModel.Gpt4O,
                contactNickname,
                NotYetAFriendPrompt(contactNickname));
            return (completion, null);
        }

        if (contact.IsNatsumeFriend is false)
        {
            completion = await GetCompletionAsync(
                NatsumeChatModel.Gpt4O,
                contactNickname,
                NotAFriendAnymorePrompt(contactNickname));
            return (completion, null);
        }

        if (contact.CurrentFriendship <= 0M)
        {
            completion = await GetCompletionAsync(
                NatsumeChatModel.Gpt4O,
                contactNickname,
                LowBalancePrompt(contactNickname));
            return (completion, null);
        }

        var imageCompletion = await openAiService.GetImageCompletionAsync(
            model.ToOpenAiModelString(),
            imageDescription
        );

        contact.AskAFavorForFriendship(
            openAiService.CalculateImageCompletionCost(
                model,
                imageCompletion.isHd,
                imageCompletion.size
            )
        );

        liteDbService.UpdateNatsumeContact(contact);

        return (null, imageCompletion.generatedImage);
    }
}