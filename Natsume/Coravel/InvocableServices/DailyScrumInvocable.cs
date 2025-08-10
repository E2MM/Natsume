using Coravel.Invocable;
using Natsume.NatsumeIntelligence;
using Natsume.NatsumeIntelligence.TextGeneration;
using Natsume.NetCord;
using Natsume.OpenAI;
using Natsume.OpenAI.Models;
using Natsume.OpenAI.NatsumeIntelligence;
using Natsume.OpenAI.OpenAI;
using Natsume.Utils;
using NetCord.Rest;

namespace Natsume.Coravel.InvocableServices;

public class DailyScrumInvocable(
    OpenAIGenerationService openAIGenerationService,
    RestClient client,
    NetCordGuildService netCordGuildService
)
    : IInvocable, ICancellableInvocable
{
    public CancellationToken CancellationToken { get; set; }

    private const string FullDateFormat = "dddd d MMMM yyyy";

    public async Task Invoke()
    {
        try
        {
            CancellationToken.ThrowIfCancellationRequested();

            await client.TriggerTypingStateAsync(
                channelId: netCordGuildService.MainChannelId,
                cancellationToken: CancellationToken
            );

            var prompt = $"{NatsumePrompt.DailyScrum} {DateTime.Now.ToString(format: FullDateFormat)}";

            var completion = await openAIGenerationService.GenerateTextAsync(
                model: TextModel.Gpt41,
                cancellationToken: CancellationToken,
                prompts:
                [
                    (ChatMessageType.System, NatsumePrompt.SystemChat),
                    (ChatMessageType.User, prompt)
                ]
            );

            await client.TriggerTypingStateAsync(
                channelId: netCordGuildService.MainChannelId,
                cancellationToken: CancellationToken
            );

            var splits = completion.GetText().SplitForDiscord();

            var message = new MessageProperties()
                .WithContent($"@everyone {splits[0]}")
                .WithAllowedMentions(new AllowedMentionsProperties().WithEveryone());

            await client.SendMessageAsync(
                channelId: netCordGuildService.MainChannelId,
                message: message,
                cancellationToken: CancellationToken
            );

            foreach (var split in splits[1..])
            {
                await client.TriggerTypingStateAsync(
                    channelId: netCordGuildService.MainChannelId,
                    cancellationToken: CancellationToken
                );

                await Task.Delay(
                    millisecondsDelay: Random.Shared.Next(2500, 5000),
                    cancellationToken: CancellationToken
                );

                message = new MessageProperties().WithContent(split);

                await client.SendMessageAsync(
                    channelId: netCordGuildService.MainChannelId,
                    message: message,
                    cancellationToken: CancellationToken
                );
            }
            Console.WriteLine(DateTime.Now);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}