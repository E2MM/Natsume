using System.Text;
using Natsume.LiteDB;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace Natsume.NetCord;

[SlashCommand("hq", "HQ commands",
    DefaultGuildUserPermissions = Permissions.Administrator,
    Contexts = [InteractionContextType.Guild, InteractionContextType.DMChannel])]
public class HqSlashCommandsModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("subscribers", "Subscribers commands")]
    public class SubscribersModule(LiteDbService liteDbService) : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand(name: "list", description: "Elenca tutti i conoscenti di Natsume-san")]
        public async Task ListSubscribers()
        {
            await RespondAsync(InteractionCallback.DeferredMessage());
            var sb = new StringBuilder(1024);
            foreach (var s in liteDbService.GetSubscribers())
            {
                sb.AppendLine($"🤓{s.Username} - {(s.ActiveSubscription ? "🤍" : "💔")} 🆔{s.Id}");
                sb.AppendLine($"💰{s.CurrentBalance:C}/{s.TotalBalanceCharged:C} (current/total)");
                sb.AppendLine($"📅Last Charge on {s.LastBalanceCharge}");
                sb.AppendLine(
                    $"💸Last on {s.LastInvocation} (count {s.TotalInvocations}) (tokens {s.InputTokensConsumed} I + {s.OutputTokensConsumed} O)");
            }

            var response = sb.ToString();
            await ModifyResponseAsync(m => m.WithContent(response));
        }
    }
}