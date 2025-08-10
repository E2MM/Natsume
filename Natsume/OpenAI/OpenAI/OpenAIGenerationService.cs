using Microsoft.Extensions.Logging;
using Natsume.NatsumeIntelligence.ImageGeneration;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.OpenAI.Models;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.OpenAI.OpenAI;

public class OpenAIGenerationService(OpenAIClientService openAIClientService, ILogger<OpenAIGenerationService> logger)
{
    // TODO: manca supporto dell'sdk alle nuove api "Responses"

    public async Task<ChatCompletion> GenerateTextAsync(
        TextModel model,
        CancellationToken cancellationToken = default,
        params IList<(ChatMessageType type, string content)> prompts
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var client = openAIClientService.GetChatClient(model: model);

        if (prompts.Any() is false) throw new Exception("No messages to send");

        var completion = await client.CompleteChatAsync(
            messages: prompts.Select(CreateChatMessage),
            cancellationToken: cancellationToken
        );

        return completion.Value;
    }

    private static ChatMessage CreateChatMessage((ChatMessageType type, string content) message)
    {
        ChatMessage chatMessage = message.type switch
        {
            ChatMessageType.System => ChatMessage.CreateSystemMessage(message.content),
            ChatMessageType.User => ChatMessage.CreateUserMessage(message.content),
            ChatMessageType.Assistant => ChatMessage.CreateAssistantMessage(message.content),
            _ => throw new ArgumentException(
                paramName: nameof(message),
                message: $"ChatMessageType '{message.type}' is not valid"
            )
        };

        return chatMessage;
    }

    public decimal GetTextGenerationCost(
        TextModel model,
        ChatCompletion completion
    )
    {
        logger.LogInformation("Total Token Count Usage: {UsageTotalTokenCount}", completion.Usage.TotalTokenCount);
        logger.LogInformation("Input Token Count Usage: {UsageInputTokenCount}", completion.Usage.InputTokenCount);
        logger.LogInformation("Output Token Count Usage: {UsageOutputTokenCount}", completion.Usage.OutputTokenCount);
        logger.LogInformation("Cached Token Count Usage: {CachedTokenCount}", completion.Usage.InputTokenDetails.CachedTokenCount);
        
        var tokenCosts = model.GetCost();
        var totalCost =completion.Usage.InputTokenCount * tokenCosts.InputTextCostPerToken
                       + completion.Usage.OutputTokenCount * tokenCosts.OutputTextCostPerToken;
        
        logger.LogInformation("Total Cost: {TotalCost}", totalCost);

        return totalCost;
    }

    public async Task<(GeneratedImage generatedImage, (int width, int heigth) size, bool isHighQuality)>
        GenerateImageAsync(
            string imagePrompt,
            ImageModel model,
            CancellationToken cancellationToken = default)
    {
        // TODO: refactorare questa roba

        cancellationToken.ThrowIfCancellationRequested();

        var client = openAIClientService.GetImageClient(model: model);

        //var quality = isHighQuality ? "high" : "medium"; // e "low" anche
        var imageOptions = new ImageGenerationOptions
        {
            //Background = GeneratedImageBackground.Auto,
            //Size = GeneratedImageSize.Auto,
            Size = new GeneratedImageSize(1024, 1024),
            //ModerationLevel = GeneratedImageModerationLevel.Low,
            //OutputCompressionFactor = 0,
            //OutputFileFormat = GeneratedImageFileFormat.Webp,
            //Quality = GeneratedImageQuality.Auto,
            Quality = new GeneratedImageQuality("medium"),
            // ResponseFormat = new GeneratedImageFormat("b64_json"), //"url"
            // Style = new GeneratedImageStyle("natural") //"vivid"
        };
        
        var result = await client.GenerateImageAsync(
            prompt: imagePrompt,
            options: imageOptions,
            cancellationToken: cancellationToken
        );

        return (result.Value, (1024, 1024), false);
    }


    public static decimal GetImageGenerationCost(
        ImageModel model,
        bool isHighQuality,
        (int width, int heigth) size
    )
    {
        // var tokenCosts = aiModel.GetCostsPerToken();
        //
        // return completion.Usage.InputTokenCount * tokenCosts.InputTextTokenCost
        //        + completion.Usage.OutputTokenCount * tokenCosts.OutputTextTokenCost;


        var longestSide = Math.Max(size.heigth, size.width);

        return (model: model, isHd: isHighQuality, longestSide) switch
        {
            (ImageModel.GptImage1, isHd: true, longestSide: > 1024) => 0.25m,
            (ImageModel.GptImage1, isHd: false, longestSide: > 1024) => 0.063m,
            (ImageModel.GptImage1, isHd: true, longestSide: <= 1024) => 0.167m,
            (ImageModel.GptImage1, isHd: false, longestSide: <= 1024) => 0.042m,
            _ => throw new ArgumentException("Invalid generation options")
        };
    }
}