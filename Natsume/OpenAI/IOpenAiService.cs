using Natsume.NetCord.NatsumeAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.OpenAI;

public interface IOpenAiService
{
    public ChatClient GetChatClient(string model);

    public Task<ChatCompletion> GetChatCompletionAsync(
        string model,
        string prompt,
        string messageContent
    );

    public Task<ChatCompletion> GetChatCompletionAsync(
        string model,
        params IEnumerable<(ChatMessageType type, string content)> messages
    );

    public Task<(GeneratedImage generatedImage, (int width, int heigth) size, bool isHd)> GetImageCompletionAsync(
        string model,
        string imageDescription
    );

    public decimal CalculateChatCompletionCost(
        NatsumeChatModel model,
        ChatCompletion completion
    );

    public decimal CalculateImageCompletionCost(
        NatsumeImageModel model,
        bool isHd,
        (int width, int heigth) size
    );
}