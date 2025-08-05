using NetCord.Services.ApplicationCommands;

namespace Natsume.NatsumeIntelligence.ImageGeneration;

public enum ImageModel
{
    [SlashCommandChoice(name: ImageModelName.GptImage1)]
    GptImage1,
}