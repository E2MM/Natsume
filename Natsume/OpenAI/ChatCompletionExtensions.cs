using OpenAI.Chat;

namespace Natsume.OpenAI;

public static class ChatCompletionExtensions
{
    public static string GetText(this ChatCompletion chatCompletion) => chatCompletion.Content[0].Text;
}