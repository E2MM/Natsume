using System.Globalization;
using Natsume.NatsumeIntelligence.ImageGeneration;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.OpenAI;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NatsumeIntelligence;

public class NatsumeAiCommandModule(NatsumeAi natsumeAi) : ApplicationCommandModule<ApplicationCommandContext>
{
    public string ContactNickname => Context.User.GlobalName ?? Context.User.Username;

    protected async Task ExecuteNatsumeCommandAsync(TextModel aiModel, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage());
        var response = await natsumeAi.GetChatCompletionTextAsync(aiModel, ContactNickname, request);
        await ModifyResponseAsync(m => m.WithContent(response));
    }

    protected async Task ExecuteFriendNatsumeReactionsAsync(TextModel aiModel, RestMessage message)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        List<string> discordReactions = [];

        var reactions = await natsumeAi.GetFriendChatCompletionReactionsAsync(
            aiModel: aiModel,
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

    protected async Task ExecuteFriendNatsumeCommandAsync(TextModel aiModel, string request)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));

        var completionText = await natsumeAi.GetFriendChatCompletionTextAsync(
            aiModel,
            Context.User.Id,
            ContactNickname,
            request
        );

        await ModifyResponseAsync(m => m.WithContent(completionText));
    }

    protected async Task ExecuteFriendNatsumeCommandAsync(ImageModel model, string imageDescription)
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