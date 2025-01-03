using Natsume.LiteDB;
using Natsume.OpenAI;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using OpenAI.Chat;

namespace Natsume.NetCord;

public class NatsumeCoreCommandModule(IOpenAiService openAiService, LiteDbService liteDbService)
    : ApplicationCommandModule<ApplicationCommandContext>
{
    private string SubscriberName => Context.User.GlobalName ?? Context.User.Username;

    [Obsolete("Here to testify Natsume's birth")]
    private string NatsumeOriginalPrompt =>
        $"""
         Sei una senior dev molto competente di nome Natsume, ti rivolgerai cordialmente a 
         {SubscriberName} usando i suffissi onorifici, cercando di aiutarli
         a migliorare il loro codice e risolvere i loro problemi. Sii amichevole e giocosa!
         """;

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

    protected async Task<ChatCompletion> GetNatsumeCompletionAsync(NatsumeLlmModel model, string request)
    {
        var completion = await openAiService.GetChatCompletion(
            model: model.ToGptModelString(),
            prompt: NatsumeBasePrompt,
            messageContent: request
        );

        return completion;
    }

    protected async Task<string> GetNatsumeCompletionTextAsync(NatsumeLlmModel model, string request)
    {
        return (await GetNatsumeCompletionAsync(model, request)).Content[0].Text;
    }

    protected async Task ExecuteNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var response = await GetNatsumeCompletionTextAsync(model, request);
        await ModifyResponseAsync(m => m.WithContent(response));
    }
    
    protected async Task ExecuteSubscribedNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var subscriber = liteDbService.GetSubscriberById(Context.User.Id);
        string response;
        if (subscriber is null)
        {
            response = await GetNatsumeCompletionTextAsync(NatsumeLlmModel.Gpt4O, NotYetASubscriberPrompt);
            await ModifyResponseAsync(m => m.WithContent(response));
            return;
        }
        
        if (subscriber.ActiveSubscription is false)
        {
            response = await GetNatsumeCompletionTextAsync(NatsumeLlmModel.Gpt4O, NotASubscriberAnymorePrompt);
            await ModifyResponseAsync(m => m.WithContent(response));
            return;
        }

        if (subscriber.CurrentBalance <= 0M)
        {
            response = await GetNatsumeCompletionTextAsync(NatsumeLlmModel.Gpt4O, LowBalancePrompt);
            await ModifyResponseAsync(m => m.WithContent(response));
            return;
        }

        var completion = await GetNatsumeCompletionAsync(model, request);
        subscriber.ConsumeBalance(
            inputTokens: completion.Usage.InputTokenCount,
            outputTokens: completion.Usage.OutputTokenCount,
            cost: openAiService.CalculateCompletionCost(model.ToGptModelString(), completion)
        );

        liteDbService.UpdateSubscriber(subscriber);

        await ModifyResponseAsync(m => m.WithContent(completion.GetText()));
    }
}