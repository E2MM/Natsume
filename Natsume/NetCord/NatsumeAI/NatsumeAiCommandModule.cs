using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAiCommandModule(NatsumeAi natsumeAi) : ApplicationCommandModule<ApplicationCommandContext>
{
    public string SubscriberName => Context.User.GlobalName ?? Context.User.Username;

    protected async Task ExecuteNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var response = await natsumeAi.GetCompletionTextAsync(model, SubscriberName, request);
        await ModifyResponseAsync(m => m.WithContent(response));
    }

    protected async Task ExecuteSubscribedNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var completionText = await natsumeAi.GetSubscribedCompletionTextAsync(
            model,
            Context.User.Id,
            SubscriberName,
            request
        );

        await ModifyResponseAsync(m => m.WithContent(completionText));
    }
}