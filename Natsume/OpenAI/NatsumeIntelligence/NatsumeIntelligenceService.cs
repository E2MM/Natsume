using System.Globalization;
using Microsoft.Extensions.Logging;
using Natsume.NatsumeIntelligence.ImageGeneration;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.NetCord.NatsumeNetCordModules;
using Natsume.OpenAI.Models;
using Natsume.OpenAI.OpenAI;
using Natsume.Utils;
using OpenAI.Images;

namespace Natsume.OpenAI.NatsumeIntelligence;

public class NatsumeIntelligenceService(OpenAIGenerationService openAIGenerationService, TimeProvider timeProvider,
        ILogger<NatsumeIntelligenceService> logger)
    //NatsumeContactService natsumeContactService)
{
    public async Task<(string generatedReply, decimal generationCost)> GenerateNatsumeReplyAsync(
        //ulong contactId,
        //string contactNickname,
        string messageContent,
        TextModel model,
        CancellationToken cancellationToken = default
    )
    {
        return await GenerateNatsumeReplyAsync(
            model: model,
            cancellationToken: cancellationToken,
            messages: new List<(ChatMessageType type, string content)>
            {
                (ChatMessageType.System, NatsumePrompt.SystemChat),
                (ChatMessageType.User, messageContent),
            }
        );
    }

    public async Task<(string generatedReply, decimal generationCost)> GenerateNatsumeReplyAsync(
        //ulong contactId,
        //string contactNickname,
        TextModel model = TextModel.Gpt41,
        CancellationToken cancellationToken = default,
        params IList<(ChatMessageType type, string content)> messages
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(GenerateNatsumeReplyAsync), 15_000);
        
        //var contact = await natsumeContactService.GetNatsumeContactByIdAsync(contactId);
        //ChatCompletion completion;

        // if (contact is null)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         NotYetAFriendPrompt(contactNickname));
        //     return completion;
        // }

        // if (contact.IsFriend is false)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         NotAFriendAnymorePrompt(contactNickname));
        //     return completion;
        // }

        // if (contact.CurrentFavor <= 0M)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         LowBalancePrompt(contactNickname));
        //     return completion;
        // }

        var completion = await openAIGenerationService.GenerateTextAsync(
            model: model,
            cancellationToken: cancellationToken,
            prompts: messages
        );

        var favorCost = openAIGenerationService.GetTextGenerationCost(
            model: model,
            completion: completion
        );

        //contact.Interact().AskAFavorForFriendship(favorCost);

        //await natsumeContactService.UpdateNatsumeContactsAsync(contacts: contact);

        return (generatedReply: completion.GetText(), favorCost);
    }

    public async Task<(List<string> generatedReactions, decimal generationCost)> GenerateNatsumeReactionsAsync(
        // ulong contactId,
        // string contactNickname,
        string messageContent,
        TextModel model = TextModel.Gpt41,
        CancellationToken cancellationToken = default
    )
    {
        using var timedScope = logger.BeginTimedOperationScope(timeProvider, nameof(GenerateNatsumeReactionsAsync), 5_000);
        
        var completion = await openAIGenerationService.GenerateTextAsync(
            model: model,
            cancellationToken: cancellationToken,
            prompts:
            [
                (ChatMessageType.System, NatsumePrompt.SystemEmojiReaction),
                (ChatMessageType.User, messageContent),
            ]
        );

        var favorCost = openAIGenerationService.GetTextGenerationCost(
            model: model,
            completion: completion
        );

        // var completion = await GetFriendChatCompletionAsync(
        //     aiModel,
        //     contactId,
        //     contactNickname,
        //     (ChatMessageType.User, NatsumePrompt.Reaction(messageContent))
        // );

        List<string> reactions = [];
        var enumerator = StringInfo.GetTextElementEnumerator(completion.GetText().Trim());
        while (enumerator.MoveNext())
        {
            var reaction = enumerator.GetTextElement();
            if (reaction.Trim() is not "" and not "👍")
            {
                reactions.Add(reaction);
            }
        }

        return (reactions.Distinct().ToList(), favorCost);
    }

    // public async Task<ChatCompletion> GetChatCompletionAsync(
    //     TextModel aiModel,
    //     params IList<(ChatMessageType type, string content)> messages
    // )
    // {
    //     var completion = await openAIGenerationService.GenerateTextAsync(
    //         model: aiModel,
    //         prompts: messages
    //     );
    //
    //     return completion;
    // }
    //
    // public async Task<string> GetChatCompletionTextAsync(
    //     TextModel aiModel,
    //     params IList<(ChatMessageType type, string content)> messages
    // )
    // {
    //     var completion = await GetChatCompletionAsync(aiModel, messages);
    //     return completion.Content[0].Text;
    // }

    // public async Task<ChatCompletion> GetCompletionAsync(
    //     TextModel aiModel,
    //     string contactNickname,
    //     string messageContent
    // )
    // {
    //     var completion = await openAIGenerationService.GenerateTextAsync(
    //         model: aiModel,
    //         prompts:
    //         new List<(ChatMessageType type, string content)>
    //         {
    //             (ChatMessageType.System, NatsumePrompt.System),
    //             (ChatMessageType.User, messageContent),
    //         }
    //     );
    //
    //     return completion;
    // }

    // public async Task<string> GetChatCompletionTextAsync(
    //     TextModel aiModel,
    //     string contactNickname,
    //     string messageContent
    // )
    // {
    //     var completion = await GetCompletionAsync(aiModel, contactNickname, messageContent);
    //     return completion.GetText();
    // }


    // public async Task<string> GetFriendChatCompletionTextAsync(
    //     TextModel aiModel,
    //     ulong contactId,
    //     string contactNickname,
    //     params IList<(ChatMessageType type, string content)> messages
    // )
    // {
    //     var completion = await GetFriendChatCompletionAsync(
    //         aiModel,
    //         contactId,
    //         contactNickname,
    //         messages
    //     );
    //
    //     return completion.GetText();
    // }

    // public async Task<ChatCompletion> GetFriendChatCompletionAsync(
    //     TextModel aiModel,
    //     ulong contactId,
    //     string contactNickname,
    //     string messageContent
    // )
    // {
    //     var completion = await GetFriendChatCompletionAsync(
    //         aiModel,
    //         contactId,
    //         contactNickname,
    //         (ChatMessageType.System, NatsumePrompt.System),
    //         (ChatMessageType.User, messageContent)
    //     );
    //
    //     return completion;
    // }

    // public async Task<string> GetFriendChatCompletionTextAsync(
    //     TextModel aiModel,
    //     ulong contactId,
    //     string contactNickname,
    //     string messageContent
    // )
    // {
    //     var completion = await GetFriendChatCompletionAsync(
    //         aiModel,
    //         contactId,
    //         contactNickname,
    //         (ChatMessageType.User, messageContent)
    //     );
    //
    //     return completion.GetText();
    // }

    // public async Task<string> GetChatCompletionReactionsAsync(
    //     TextModel aiModel,
    //     string messageContent
    // )
    // {
    //     var completion = await GetChatCompletionAsync(
    //         aiModel,
    //         (ChatMessageType.User, ReactionPrompt(messageContent))
    //     );
    //
    //     return completion.GetText().Trim();
    // }

    // public async Task<string> GetFriendChatCompletionReactionsAsync(
    //     TextModel aiModel,
    //     ulong contactId,
    //     string contactNickname,
    //     string messageContent
    // )
    // {
    //     var completion = await GetFriendChatCompletionAsync(
    //         aiModel,
    //         contactId,
    //         contactNickname,
    //         (ChatMessageType.User, ReactionPrompt(messageContent))
    //     );
    //
    //     return completion.GetText().Trim();
    // }

    public async Task<GeneratedImage> GetFriendImageCompletionAsync(
        ImageModel model,
        //ulong contactId,
        //string contactNickname,
        string imageDescription,
        CancellationToken cancellationToken = default
    )
    {
        var completion = await openAIGenerationService.GenerateImageAsync(
            model: model,
            cancellationToken: cancellationToken,
            imagePrompt: imageDescription
        );

        var favorCost = OpenAIGenerationService.GetImageGenerationCost(
            model: model,
            isHighQuality: completion.isHighQuality,
            size: completion.size
        );

        // var contact = await natsumeContactService.GetNatsumeContactByIdAsync(contactId);
        // ChatCompletion completion;

        // if (contact is null)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         NotYetAFriendPrompt(contactNickname));
        //     return (completion, null);
        // }
        //
        // if (contact.IsFriend is false)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         NotAFriendAnymorePrompt(contactNickname));
        //     return (completion, null);
        // }
        //
        // if (contact.CurrentFavor <= 0M)
        // {
        //     completion = await GetCompletionAsync(
        //         TextModel.Gpt41,
        //         contactNickname,
        //         LowBalancePrompt(contactNickname));
        //     return (completion, null);
        // }

        // var imageCompletion = await openAIGenerationService.GenerateImageAsync(
        //     model: model,
        //     imagePrompt: imageDescription
        // );
        //
        // var favorCost = OpenAIGenerationService.GetImageGenerationCost(
        //     model,
        //     imageCompletion.isHighQuality,
        //     imageCompletion.size
        // );

        // contact.Interact().AskAFavorForFriendship(favorCost);
        //
        // await natsumeContactService.UpdateNatsumeContactsAsync(contacts: contact);

        return completion.generatedImage;
    }
}