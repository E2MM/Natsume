using Natsume.NatsumeIntelligence.ImageGeneration;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.OpenAI.Models;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.OpenAI.OpenAI;

public class OpenAIClientService(string apiKey)
{
    private readonly OpenAIClient _openAiClient = new(apiKey: apiKey);

    public OpenAIClient GetOpenAIClient()
    {
        return _openAiClient;
    }

    public ChatClient GetChatClient(TextModel model)
    {
        return _openAiClient.GetChatClient(model: model.GetName());
    }

    public ImageClient GetImageClient(ImageModel model)
    {
        return _openAiClient.GetImageClient(model: model.GetName());
    }

    // TODO: manca supporto dell'sdk alle nuove api "Responses"

    public async Task<(GeneratedImage generatedImage, (int width, int heigth) size, bool isHighQuality)>
        GetImageCompletionAsync(
            ImageModel model,
            string imageDescription
        )
    {
        // TODO: refactorare questa roba
        var client = GetImageClient(model);
        //var quality = isHighQuality ? "high" : "medium";
        var result = await client.GenerateImageAsync(imageDescription, new ImageGenerationOptions
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
        });

        return (result.Value, (1024, 1024), false);
    }

    public async Task<ChatCompletion> GetChatCompletionAsync(
        TextModel aiModel,
        string prompt,
        string messageContent
    )
    {
        var client = GetChatClient(aiModel);
        var promptMessage = ChatMessage.CreateSystemMessage(prompt);
        var userMessage = ChatMessage.CreateUserMessage(messageContent);

        ChatCompletion completion = await client.CompleteChatAsync(promptMessage, userMessage);

        return completion;
    }

    private static ChatMessage CreateChatMessage((ChatMessageType type, string content) message)
    {
        ChatMessage chatMessage = message.type switch
        {
            ChatMessageType.System => ChatMessage.CreateSystemMessage(message.content),
            ChatMessageType.User => ChatMessage.CreateUserMessage(message.content),
            ChatMessageType.Assistant => ChatMessage.CreateAssistantMessage(message.content),
            _ => throw new ArgumentException("Invalid message type")
        };

        return chatMessage;
    }

    public async Task<ChatCompletion> GetChatCompletionAsync(
        TextModel aiModel,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        var client = GetChatClient(aiModel);
        List<ChatMessage> chatMessages = [];
        chatMessages.AddRange(messages.Select(CreateChatMessage));

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages);
        return completion;
    }

    public decimal CalculateChatCompletionCost(
        TextModel aiModel,
        ChatCompletion completion
    )
    {
        var tokenCosts = aiModel.GetCost();

        return completion.Usage.InputTokenCount * tokenCosts.InputTextCostPerToken
               + completion.Usage.OutputTokenCount * tokenCosts.OutputTextCostPerToken;
    }

    public decimal CalculateImageCompletionCost(
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