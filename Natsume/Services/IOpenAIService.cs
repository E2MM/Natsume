using OpenAI.Chat;

namespace Natsume.Services;

public interface IOpenAIService
{
    public ChatClient GetChatClient();
}