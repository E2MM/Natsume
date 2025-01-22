namespace Natsume.NetCord.NatsumeAI;

public static class NatsumeChatModelExtensions
{
    public static string ToOpenAiModelString(this NatsumeChatModel model)
    {
        return model switch
        {
            NatsumeChatModel.Gpt4O => "gpt-4o",
            NatsumeChatModel.Gpt4OMini => "gpt-4o-mini",
            NatsumeChatModel.O1 => "o1",
            NatsumeChatModel.O1Mini => "o1-mini",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "model does not exist")
        };
    }
}