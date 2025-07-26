using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsume.Coravel;
using Natsume.Database;
using Natsume.NetCord.NatsumeAI;
using Natsume.NetCord.NatsumeNetCordModules;
using Natsume.OpenAI;
using Natsume.Services;
using Natsume.Utils;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

var discordToken = builder.Configuration.GetValueOrThrow("Discord", "Token");
var openAiApiKey = builder.Configuration.GetValueOrThrow("OpenAI", "ApiKey");
var sqliteConnection = builder.Configuration.GetValueOrThrow("SQLite", "ConnectionString");

builder.AddDbServices(sqliteConnection);
builder.AddInvocableServices();

builder.Services
    .AddScheduler()
    .AddDiscordGateway(options =>
        {
            options.Token = discordToken;
            options.Intents =
                GatewayIntents.GuildMessages
                | GatewayIntents.DirectMessages
                | GatewayIntents.MessageContent;
        }
    )
    .AddGatewayEventHandler<NatsumeListeningModule>()
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext>()
    .AddScoped<IOpenAiService, OpenAiService>(_ => new OpenAiService(openAiApiKey))
    .AddScoped<NatsumeAi>();

var host = builder
    .Build()
    .UseGatewayEventHandlers()
    .AddApplicationCommandModule<NatsumeCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule.NatsumeContactsModule>()
    .AddApplicationCommandModule<NatsumeHqUserCommandModule>()
    .AddApplicationCommandModule<NatsumeGoogleMeetCommandModule>()
    .AddApplicationCommandModule<NatsumeRemindMeCommandModule>();

UseScheduledInvocableServices();
var cts = new CancellationTokenSource();
await MigrateDatabaseAsync(cts.Token);
await host.RunAsync(cts.Token);

return;

async Task MigrateDatabaseAsync(CancellationToken token)
{
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NatsumeDbContext>();
    await db.Database.MigrateAsync(token);
}

ISchedulerConfiguration UseScheduledInvocableServices()
{
    return host.Services.UseScheduler(scheduler =>
        {
            scheduler
                .Schedule<BondUpInvocable>()
                .Hourly();

            scheduler
                .Schedule<RemindMeInvocable>()
                .EveryMinute();
        }
    );
}
