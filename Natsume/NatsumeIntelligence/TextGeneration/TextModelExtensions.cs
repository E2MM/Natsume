namespace Natsume.NatsumeIntelligence.TextGeneration;

public static class TextModelExtensions
{
    public static string GetName(this TextModel model)
    {
        return model switch
        {
            TextModel.Gpt5 => TextModelName.Gpt5,
            TextModel.Gpt41 => TextModelName.Gpt41,
            TextModel.O3 => TextModelName.O3,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(model),
                actualValue: model,
                message: $"Model '{model}' is not supported"
            )
        };
    }

    private const decimal PerMillion = 1_000_000;

    public static TextModelCost GetCost(this TextModel model)
    {
        return model switch
        {
            TextModel.Gpt5 => new TextModelCost
            {
                InputTextCostPerToken = 1.25M / PerMillion,
                OutputTextCostPerToken = 10M / PerMillion
            },
            TextModel.Gpt41 => new TextModelCost
            {
                InputTextCostPerToken = 2M / PerMillion,
                OutputTextCostPerToken = 8M / PerMillion
            },
            TextModel.O3 => new TextModelCost
            {
                InputTextCostPerToken = 2M / PerMillion,
                OutputTextCostPerToken = 8M / PerMillion
            },
            _ => throw new ArgumentException(
                paramName: nameof(model),
                message: $"Model '{model}' is not valid"
            )
        };
    }
}