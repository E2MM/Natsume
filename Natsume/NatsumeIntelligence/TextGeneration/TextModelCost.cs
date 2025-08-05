namespace Natsume.NatsumeIntelligence.TextGeneration;

public readonly record struct TextModelCost(
    decimal InputTextCostPerToken,
    decimal OutputTextCostPerToken
);