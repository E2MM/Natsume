using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Natsume.Coravel;
using Natsume.NatsumeIntelligence;
using Natsume.NetCord;
using Natsume.NetCord.NatsumeNetCordModules;
using Natsume.OpenAI.NatsumeIntelligence;
using Natsume.OpenAI.OpenAI;
using Natsume.Persistence;
using Natsume.Utils;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;


var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();


var discordToken = builder.Configuration.GetStringValueOrThrow("Discord", "Token");
var guildId = builder.Configuration.GetUlongValueOrThrow("Discord", "GuildId");
var generalChannelId = builder.Configuration.GetUlongValueOrThrow("Discord", "GeneralChannelId");
var openAiApiKey = builder.Configuration.GetStringValueOrThrow("OpenAI", "ApiKey");
var sqliteConnection = builder.Configuration.GetStringValueOrThrow("SQLite", "ConnectionString");

builder.Services
    .AddPersistenceServices(sqliteConnection: sqliteConnection)
    .AddCoravelInvocableServices()
    .AddSingleton<NetCordGuildService>(_ => new NetCordGuildService(guildId: guildId, mainChannelId: generalChannelId))
    .AddSingleton<OpenAIClientService>(_ => new OpenAIClientService(apiKey: openAiApiKey))
    .AddSingleton<OpenAIGenerationService>()
    .AddSingleton<NatsumeIntelligenceService>()
    .AddSingleton(TimeProvider.System);

builder.Services
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
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext>();

var host = builder
    .Build()
    .UseGatewayEventHandlers()
    //.AddApplicationCommandModule<NatsumeCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule.NatsumeContactsModule>()
    //.AddApplicationCommandModule<NatsumeHqUserCommandModule>()
    .AddApplicationCommandModule<NatsumeGoogleMeetCommandModule>()
    .AddApplicationCommandModule<NatsumeRemindMeCommandModule>();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Natsume is getting ready...");

var cts = new CancellationTokenSource();
host.UseCoravelScheduledInvocableServices();
await host.MigrateDatabaseAsync(cts.Token);

logger.LogInformation("Natsume is ready!");
await host.RunAsync(cts.Token);