using OpenAI.Chat;

namespace Natsume.Services;

public class OpenAIService(string apiKey) : IOpenAIService
{
    public ChatClient GetChatClient()
    {
        return new ChatClient(model: "gpt-4o", apiKey: apiKey);
    }
}