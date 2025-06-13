using System.Globalization;
using Natsume.OpenAI;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

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

    protected async Task ExecuteFriendNatsumeReactionsAsync(NatsumeChatModel model, RestMessage message)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        List<string> discordReactions = [];

        var reactions = await natsumeAi.GetFriendChatCompletionReactionsAsync(
            model: model,
            contactId: Context.User.Id,
            contactNickname: ContactNickname,
            messageContent: message.Content
        );

        var enumerator = StringInfo.GetTextElementEnumerator(reactions);
        while (enumerator.MoveNext())
        {
            var reaction = enumerator.GetTextElement();
            if (reaction.Trim() != string.Empty)
            {
                discordReactions.Add(enumerator.GetTextElement());
            }
        }

        discordReactions = discordReactions.Distinct().ToList();

        foreach (var discordReaction in discordReactions)
        {
            try
            {
                await message.AddReactionAsync(new ReactionEmojiProperties(discordReaction));
            }
            catch
            {
                Console.WriteLine($"Natsume's reaction \"{discordReaction}\" is not a valid Discord reaction");
            }
        }

        await ModifyResponseAsync(m => m.WithContent(string.Concat(discordReactions)));
    }

    protected async Task ExecuteFriendNatsumeCommandAsync(NatsumeChatModel model, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

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

        if (completion.generatedImage is null && completion.chatCompletion is null)
        {
            await ModifyResponseAsync(m => m.WithContent("Scusa sono crashata! 😱"));
        }
    }
}