using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAiCommandModule(NatsumeAi natsumeAi) : ApplicationCommandModule<ApplicationCommandContext>
{
    public string ContactNickname => Context.User.GlobalName ?? Context.User.Username;

    protected async Task ExecuteNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var response = await natsumeAi.GetCompletionTextAsync(model, ContactNickname, request);
        await ModifyResponseAsync(m => m.WithContent(response));
    }

    protected async Task ExecuteFriendNatsumeCommandAsync(NatsumeLlmModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var completionText = await natsumeAi.GetFriendCompletionTextAsync(
            model,
            Context.User.Id,
            ContactNickname,
            request
        );

        await ModifyResponseAsync(m => m.WithContent(completionText));
    }
}