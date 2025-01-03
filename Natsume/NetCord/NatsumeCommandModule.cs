using Natsume.OpenAI;
using Natsume.LiteDB;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord;

public class NatsumeCommandModule(IOpenAiService openAiService, LiteDbService liteDbService)
    : NatsumeCoreCommandModule(openAiService, liteDbService)
{
    [SlashCommand(name: "chat", description: "Chatta con Natsume-san!")]
    public async Task Chat(
        [SlashCommandParameter(Name = "messaggio", Description = "Scrivi il tuo messaggio a Natsume-san")]
        string message)
    {
        await ExecuteSubscribedNatsumeCommandAsync(NatsumeLlmModel.Gpt4O, message);
    }
    
    [SlashCommand(name: "aiutami", description: "Chiedi l'esperta consulenza tecnica di Natsume-san!")]
    public async Task HelpMe(
        [SlashCommandParameter(Name = "llm", Description = "Scegli il modello LLM da usare")]
        NatsumeLlmModel model,
        [SlashCommandParameter(Name = "richiesta", Description = "Scrivi la tua richiesta per Natsume-san")]
        string request)
    {
        var messageContent =
            $"""
             Natsume-san, per favore, aiutami in questa mia richiesta!
             {request}
             """;

        await ExecuteSubscribedNatsumeCommandAsync(model, messageContent);
    }

    [MessageCommand(name: "Natsume-san, non ho capito!")]
    public async Task ExplainMe(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, non ho capito! T_T Per favore, spiegami cosa c'è scritto!
             {restMessage.Content}
             """;

        await ExecuteSubscribedNatsumeCommandAsync(NatsumeLlmModel.Gpt4O, messageContent);
    }

    [MessageCommand(name: "Natsume-san, cosa ne pensi?")]
    public async Task Elaborate(RestMessage restMessage)
    {
        var messageContent =
            $"""
             Natsume-san, vorrei la tua opinione, cosa ne pensi?
             {restMessage.Content}
             """;

        await ExecuteSubscribedNatsumeCommandAsync(NatsumeLlmModel.Gpt4O, messageContent);
    }
}