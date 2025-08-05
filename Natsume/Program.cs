using Coravel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsume.Coravel;
using Natsume.NatsumeIntelligence;
using Natsume.NetCord;
using Natsume.NetCord.NatsumeNetCordModules;
using Natsume.OpenAI;
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
    .AddDatabaseServices(sqliteConnection: sqliteConnection)
    .AddInvocableServices()
    .AddSingleton<NetCordGuildService>(_ => new NetCordGuildService(guildId: guildId, mainChannelId: generalChannelId))
    .AddSingleton<OpenAIClientService>(_ => new OpenAIClientService(apiKey: openAiApiKey))
    .AddSingleton<OpenAIGenerationService>();

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

host.Services.UseScheduledInvocableServices();

var cts = new CancellationTokenSource();

await host.Services.MigrateDatabaseAsync(cts.Token);

await host.RunAsync(cts.Token);