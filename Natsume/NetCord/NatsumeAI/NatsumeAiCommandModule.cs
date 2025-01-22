using Natsume.OpenAI;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using OpenAI.Chat;
using OpenAI.Images;

namespace Natsume.NetCord.NatsumeAI;

public class NatsumeAiCommandModule(NatsumeAi natsumeAi) : ApplicationCommandModule<ApplicationCommandContext>
{
    public string ContactNickname => Context.User.GlobalName ?? Context.User.Username;

    protected async Task ExecuteNatsumeCommandAsync(NatsumeChatModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var response = await natsumeAi.GetChatCompletionTextAsync(model, ContactNickname, request);
        await ModifyResponseAsync(m => m.WithContent(response));
    }

    protected async Task ExecuteFriendNatsumeCommandAsync(NatsumeChatModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var completionText = await natsumeAi.GetFriendChatCompletionTextAsync(
            model,
            Context.User.Id,
            ContactNickname,
            request
        );

        await ModifyResponseAsync(m => m.WithContent(completionText));
    }

    protected async Task ExecuteFriendNatsumeCommandAsync(NatsumeImageModel model, string imageDescription)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());

        var completion = await natsumeAi.GetFriendImageCompletionAsync(
            model,
            Context.User.Id,
            ContactNickname,
            imageDescription
        );

        if (completion.chatCompletion is not null)
        {
            await ModifyResponseAsync(m => m.WithContent(completion.chatCompletion.GetText()));
        }

        if (completion.generatedImage is not null)
        {
            await ModifyResponseAsync(m => m.AddAttachments(
                new AttachmentProperties("image.jpg", completion.generatedImage.ImageBytes.ToStream())));
        }
    }
}