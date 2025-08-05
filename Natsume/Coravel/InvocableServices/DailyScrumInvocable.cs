using Coravel.Invocable;
using Natsume.NatsumeIntelligence;
using Natsume.NetCord;
using Natsume.OpenAI;
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

    private const string DailyPrompt =
        """
        Prepara un breve messaggio giornaliero per lanciare l'inizio della giornata di sviluppo 
        e il Daily Scrum (ricordando cosa fare in modo propositivo), 
        tenendo in considerazione la data e qualche potenziale anniversario: oggi è 
        """;

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

            var prompt = $"{DailyPrompt} {DateTime.Now.ToString(format: FullDateFormat)}";

            var completion = await openAIGenerationService.GetTextAsync(
                cancellationToken: CancellationToken,
                prompts:
                [
                    (ChatMessageType.System, NatsumeAi.SystemPrompt),
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
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}