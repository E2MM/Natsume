namespace Natsume.NatsumeIntelligence.ImageGeneration;

public readonly record struct ImageModelCost(
    decimal InputTextCostPerToken,
    decimal InputImageCostPerToken,
    decimal OutputImageCostPerToken
);