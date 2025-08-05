using NetCord.Services.ApplicationCommands;

namespace Natsume.NatsumeIntelligence.TextGeneration;

public enum TextModel
{
    [SlashCommandChoice(name: TextModelName.Gpt41)]
    Gpt41,

    [SlashCommandChoice(name: TextModelName.O3)]
    O3,
}