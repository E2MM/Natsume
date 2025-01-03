using OpenAI.Chat;

namespace Natsume.OpenAI;

public interface IOpenAiService
{
    public ChatClient GetChatClient(string model);
    public Task<ChatCompletion> GetChatCompletion(string model, string prompt, string messageContent);
    public Task<ChatCompletion> GetChatCompletion(string model,
        params IEnumerable<(ChatMessageType type, string content)> messages);
    public decimal CalculateCompletionCost(string model, ChatCompletion completion);

}