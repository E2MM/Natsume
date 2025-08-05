namespace Natsume.NatsumeIntelligence.ImageGeneration;

public static class ImageModelExtensions
{
    public static string GetName(this ImageModel model)
    {
        return model switch
        {
            ImageModel.GptImage1 => ImageModelName.GptImage1,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(model),
                actualValue: model,
                message: $"Model '{model}' is not supported"
            )
        };
    }

    private const decimal PerMillion = 1_000_000;

    public static ImageModelCost GetCost(this ImageModel model)
    {
        return model switch
        {
            ImageModel.GptImage1 => new ImageModelCost
            {
                InputTextCostPerToken = 5M / PerMillion,
                InputImageCostPerToken = 10M / PerMillion,
                OutputImageCostPerToken = 40M / PerMillion
            },
            _ => throw new ArgumentException(
                paramName: nameof(model),
                message: $"Model '{model}' is not valid"
            )
        };
    }
}