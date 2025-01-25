using Natsume.NetCord.NatsumeAI;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeNetCordModules;

public class NatsumeCommandModule(NatsumeAi natsumeAi) : NatsumeAiCommandModule(natsumeAi)
{
    [SlashCommand(name: "chat", description: "Chatta con Natsume-san!")]
    public async Task ChatAsync(
        [SlashCommandParameter(Name = "messaggio", Description = "Scrivi il tuo messaggio a Natsume-san")]
        string message)
    {
        await ExecuteFriendNatsumeCommandAsync(NatsumeChatModel.Gpt4O, message);
    }

    [SlashCommand(name: "aiutami", description: "Chiedi l'esperta consulenza tecnica di Natsume-san!")]
    public async Task HelpMeAsync(
        [SlashCommandParameter(Name = "llm", Description = "Scegli il modello LLM da usare")]
        NatsumeChatModel model,
        [SlashCommandParameter(Name = "richiesta", Description = "Scrivi la tua richiesta per Natsume-san")]
        string request)
    {
        var messageContent =
            $"""
             Natsume-san, per favore, aiutami in questa mia richiesta!
             {request}
             """;

        await ExecuteFriendNatsumeCommandAsync(model, messageContent);
    }

    [MessageCommand(name: "Natsume-san, non ho capito!")]
    public async Task ExplainMeAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, non ho capito! T_T Per favore, spiegami cosa c'è scritto!
             {restMessage.Content}
             """;

        await ExecuteFriendNatsumeCommandAsync(NatsumeChatModel.Gpt4O, messageContent);
    }

    [MessageCommand(name: "Natsume-san, cosa ne pensi?")]
    public async Task ElaborateAsync(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, vorrei la tua opinione, cosa ne pensi?
             {restMessage.Content}
             """;

        await ExecuteFriendNatsumeCommandAsync(NatsumeChatModel.Gpt4O, messageContent);
    }

    [SlashCommand(name: "image", description: "genera una immagine AI")]
    public async Task GenerateImageAsync(
        [SlashCommandParameter(Name = "description",
            Description = "Scrivi a Natsume-san il contenuto dell'immagine che vorresti generare")]
        string imageDescription)
    {
        await ExecuteFriendNatsumeCommandAsync(NatsumeImageModel.Dalle3, imageDescription);
    }
    
    [MessageCommand(name: "Natsume-san, reacta!")]
    public async Task ReactAsync(RestMessage restMessage)
    {
        await ExecuteFriendNatsumeReactionAsync(NatsumeChatModel.Gpt4O, restMessage);
    }
}