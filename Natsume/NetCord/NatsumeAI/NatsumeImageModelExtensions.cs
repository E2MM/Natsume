namespace Natsume.NetCord.NatsumeAI;

public static class NatsumeImageModelExtensions
{
    public static string ToOpenAiModelString(this NatsumeImageModel model)
    {
        return model switch
        {
            NatsumeImageModel.Dalle3 => "dall-e-3",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "model does not exist")
        };
    }
}