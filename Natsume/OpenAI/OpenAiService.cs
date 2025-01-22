using Natsume.NetCord.NatsumeAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.OpenAI;

public class OpenAiService(string apiKey) : IOpenAiService
{
    private static bool IsValidChatModel(string model)
    {
        return model switch
        {
            "gpt-4o" => true,
            "gpt-4o-mini" => true,
            "o1" => true,
            "o1-mini" => true,
            _ => false
        };
    }

    private static bool IsValidImageModel(string model)
    {
        return model switch
        {
            "dall-e-3" => true,
            "DALL·E 3" => true,
            "dall-e-2" => true,
            "DALL·E 2" => true,
            _ => false
        };
    }

    public ImageClient GetImageClient(string model)
    {
        if (!IsValidImageModel(model)) throw new ArgumentException("Invalid image model");
        return new ImageClient(model: model, apiKey: apiKey);
    }

    public ChatClient GetChatClient(string model)
    {
        if (!IsValidChatModel(model)) throw new ArgumentException("Invalid model");
        return new ChatClient(model: model, apiKey: apiKey);
    }

    public async Task<(GeneratedImage generatedImage, (int width, int heigth) size, bool isHd)> GetImageCompletionAsync(
        string model,
        string imageDescription)
    {
        // TODO: refactorare questa roba
        if (!IsValidImageModel(model)) throw new ArgumentException("Invalid image model");
        var client = GetImageClient(model);
        var result = await client.GenerateImageAsync(imageDescription, new ImageGenerationOptions
        {
            Size = new GeneratedImageSize(1024, 1024),
            ResponseFormat = new GeneratedImageFormat("b64_json"), //"url"
            Style = new GeneratedImageStyle("natural") //"vivid"
            //Quality = new GeneratedImageQuality("hd")
        });

        return (result.Value, (1024, 1024), false);
    }

    public async Task<ChatCompletion> GetChatCompletionAsync(
        string model,
        string prompt,
        string messageContent
    )
    {
        if (!IsValidChatModel(model)) throw new ArgumentException("Invalid model");
        var client = GetChatClient(model);
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
        string model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    )
    {
        if (!IsValidChatModel(model)) throw new ArgumentException("Invalid model");
        var client = GetChatClient(model);
        List<ChatMessage> chatMessages = [];
        chatMessages.AddRange(messages.Select(CreateChatMessage));

        ChatCompletion completion = await client.CompleteChatAsync(chatMessages);
        return completion;
    }

    public decimal CalculateChatCompletionCost(
        NatsumeChatModel model,
        ChatCompletion completion
    )
    {
        var (inputTokenCostPerMillion, outputTokenCostPerMillion) = model switch
        {
            NatsumeChatModel.Gpt4O => (2.50M, 10M),
            NatsumeChatModel.Gpt4OMini => (0.15M, 0.6M),
            NatsumeChatModel.O1 => (15M, 60M),
            NatsumeChatModel.O1Mini => (3M, 12M),
            _ => throw new ArgumentException("Invalid model")
        };

        return completion.Usage.InputTokenCount * inputTokenCostPerMillion / 1000000
               + completion.Usage.OutputTokenCount * outputTokenCostPerMillion / 1000000;
    }

    public decimal CalculateImageCompletionCost(
        NatsumeImageModel model,
        bool isHd,
        (int width, int heigth) size
    )
    {
        var longestSide = Math.Max(size.heigth, size.width);

        return (model, isHd, longestSide) switch
        {
            (NatsumeImageModel.Dalle3, isHd: true, longestSide: > 1024) => 0.12m,
            (NatsumeImageModel.Dalle3, isHd: false, longestSide: > 1024) => 0.08m,
            (NatsumeImageModel.Dalle3, isHd: true, longestSide: <= 1024) => 0.08m,
            (NatsumeImageModel.Dalle3, isHd: false, longestSide: <= 1024) => 0.04m,
            _ => throw new ArgumentException("Invalid generation options")
        };
    }
}