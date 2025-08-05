using Natsume.NatsumeIntelligence;
using Natsume.NatsumeIntelligence.ImageGeneration;
using Natsume.NatsumeIntelligence.TextGeneration;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

internal class NatsumeCommandModule(NatsumeAi natsumeAi) : NatsumeAiCommandModule(natsumeAi)
{
    [SlashCommand(name: "chat", description: "Chatta con Natsume-san!")]
    public async Task ChatAsync(
        [SlashCommandParameter(Name = "messaggio", Description = "Scrivi il tuo messaggio a Natsume-san")]
        string message)
    {
        await ExecuteFriendNatsumeCommandAsync(TextModel.Gpt41, message);
    }

    [SlashCommand(name: "aiutami", description: "Chiedi l'esperta consulenza tecnica di Natsume-san!")]
    public async Task HelpMeAsync(
        [SlashCommandParameter(Name = "llm", Description = "Scegli il modello LLM da usare")]
        TextModel aiModel,
        [SlashCommandParameter(Name = "richiesta", Description = "Scrivi la tua richiesta per Natsume-san")]
        string request)
    {
        var messageContent =
            $"""
             Natsume-san, per favore, aiutami in questa mia richiesta!
             {request}
             """;

        await ExecuteFriendNatsumeCommandAsync(aiModel, messageContent);
    }

    [MessageCommand(name: "Natsume-san, non ho capito!")]
    public async Task ExplainMeAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, non ho capito! T_T Per favore, spiegami cosa c'è scritto!
             {restMessage.Content}
             """;

        await ExecuteFriendNatsumeCommandAsync(TextModel.Gpt41, messageContent);
    }

    [MessageCommand(name: "Natsume-san, cosa ne pensi?")]
    public async Task ElaborateAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, vorrei la tua opinione, cosa ne pensi?
             {restMessage.Content}
             """;

        await ExecuteFriendNatsumeCommandAsync(TextModel.Gpt41, messageContent);
    }

    [SlashCommand(name: "image", description: "genera una immagine AI")]
    public async Task GenerateImageAsync(
        [SlashCommandParameter(Name = "description",
            Description = "Scrivi a Natsume-san il contenuto dell'immagine che vorresti generare")]
        string imageDescription)
    {
        await ExecuteFriendNatsumeCommandAsync(ImageModel.GptImage1, imageDescription);
    }

    [MessageCommand(name: "Natsume-san, reacta!")]
    public async Task ReactAsync(RestMessage restMessage)
    {
        await ExecuteFriendNatsumeReactionsAsync(TextModel.Gpt41, restMessage);
    }

    [MessageCommand(name: "Natsume-san, ELI5")]
    public async Task ExplainLikeImFiveAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, ELI5! 
             Per favore, spiegami nel mondo più semplice possibile il contenuto del seguente messaggio,
             assumendo che io non abbia pressoché nessuna conoscenza della materia in oggetto:
             
             {restMessage.Content}
             """;
        
        await ExecuteFriendNatsumeCommandAsync(TextModel.Gpt41, messageContent);
    }

    [MessageCommand(name: "Natsume-san, riassumi")]
    public async Task SummarizeAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, per favore, riassumi il seguente messaggio proponendo una sintesi strutturata che
             evidenzi i punti principali (al massimo tre), e nel modo più sintetico possibile 
             (non più di due o tre righe per punto):

             {restMessage.Content}
             """;
        
            await ExecuteFriendNatsumeCommandAsync(TextModel.Gpt41, messageContent);
    }
}