using OpenAI.Chat;

namespace Natsume.OpenAI;

public class OpenAiService(string apiKey) : IOpenAiService
{
    private static bool IsValidModel(string model)
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
    
    public ChatClient GetChatClient(string model)
    {
        if (!IsValidModel(model)) throw new ArgumentException("Invalid model");
        return new ChatClient(model: model, apiKey: apiKey);
    }

    public async Task<ChatCompletion> GetChatCompletion(string model, string prompt, string messageContent)
    {
        if (!IsValidModel(model)) throw new ArgumentException("Invalid model");
        var client = GetChatClient(model);
        var promptMessage = ChatMessage.CreateSystemMessage(prompt);
        var userMessage = ChatMessage.CreateUserMessage(messageContent);
        
        ChatCompletion completion = await client.CompleteChatAsync(promptMessage, userMessage);
    
        return completion;
    }

    public async Task<ChatCompletion> GetChatCompletion(string model, params IEnumerable<(ChatMessageType type, string content)> messages)
    {
        if (!IsValidModel(model)) throw new ArgumentException("Invalid model");
        var client = GetChatClient(model);
        List<ChatMessage> chatMessages = [];
        
        foreach (var message in messages)
        {
            ChatMessage chatMessage = message.type switch
            {
                ChatMessageType.System => ChatMessage.CreateSystemMessage(message.content),
                ChatMessageType.User => ChatMessage.CreateUserMessage(message.content),
                ChatMessageType.Assistant => ChatMessage.CreateAssistantMessage(message.content),
                _ => throw new ArgumentException("Invalid message type")
            };
            
            chatMessages.Add(chatMessage);
        }
        
        ChatCompletion completion = await client.CompleteChatAsync(chatMessages);
        return completion;
    }
    
    public decimal CalculateCompletionCost(string model, ChatCompletion completion)
    {
        var (inputTokenCostPerMillion, outputTokenCostPerMillion) = model switch
        {
            "gpt-4o" => (2.50M, 10M),
            "gpt-4o-mini" => (0.15M, 0.6M),
            "o1" => (15M, 60M),
            "o1-mini" => (3M, 12M),
            _ => throw new ArgumentException("Invalid model")
        };
        
        return completion.Usage.InputTokenCount * inputTokenCostPerMillion / 1000000 
               + completion.Usage.OutputTokenCount * outputTokenCostPerMillion / 1000000;
    }

}