using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeAI;

public enum NatsumeLlmModel
{
    [SlashCommandChoice("gpt-4o")] Gpt4O,
    [SlashCommandChoice("gpt-4o-mini")] Gpt4OMini,
    [SlashCommandChoice("o1")] O1,
    [SlashCommandChoice("o1-mini")] O1Mini
}