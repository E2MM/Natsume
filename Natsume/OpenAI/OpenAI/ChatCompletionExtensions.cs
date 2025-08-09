using OpenAI.Chat;

namespace Natsume.OpenAI.Models;

public static class ChatCompletionExtensions
{
    public static string GetText(this ChatCompletion chatCompletion) => chatCompletion.Content[0].Text;
}