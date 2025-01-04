namespace Natsume.NetCord.NatsumeAI;

public static class NatsumeLlmModelExtensions
{
    public static string ToGptModelString(this NatsumeLlmModel model)
    {
        return model switch
        {
            NatsumeLlmModel.Gpt4O => "gpt-4o",
            NatsumeLlmModel.Gpt4OMini => "gpt-4o-mini",
            NatsumeLlmModel.O1 => "o1",
            NatsumeLlmModel.O1Mini => "o1-mini",
            _ => throw new ArgumentOutOfRangeException(nameof(model), model, "model does not exist")
        };
    }
}