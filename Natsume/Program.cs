using Coravel;
using Coravel.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Natsume.Coravel;
using Natsume.LiteDB;
using Natsume.NetCord.NatsumeAI;
using Natsume.NetCord.NatsumeNetCordModules;
using Natsume.OpenAI;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Services.ApplicationCommands;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddUserSecrets<Program>();

var discordToken = builder.Configuration.GetSection("Discord")["Token"];
if (discordToken is null or "") throw new ApplicationException("Invalid Discord Token");

var openAiApiKey = builder.Configuration.GetSection("OpenAI")["ApiKey"];
if (openAiApiKey is null or "") throw new ApplicationException("Invalid OpenAI Api Key");

var liteDbConnection = builder.Configuration.GetSection("LiteDB")["ConnectionString"];
if (liteDbConnection is null or "") throw new ApplicationException("Invalid LiteDB Connection String");

builder.Services
    .AddScheduler()
    .AddDiscordGateway(options =>
    {
        options.Token = discordToken;
        options.Intents =
            GatewayIntents.GuildMessages
            | GatewayIntents.DirectMessages
            | GatewayIntents.MessageContent;
    })
    .AddGatewayEventHandler<NatsumeListeningModule>()
    .AddApplicationCommands<ApplicationCommandInteraction, ApplicationCommandContext>()
    .AddSingleton<IOpenAiService, OpenAiService>(_ => new OpenAiService(openAiApiKey))
    .AddSingleton<LiteDbService>(_ => new LiteDbService(liteDbConnection))
    .AddSingleton<NatsumeAi>()
    .AddTransient<BondUpInvocable>();

var host = builder
    .Build()
    .UseGatewayEventHandlers()
    .AddApplicationCommandModule<NatsumeCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule.NatsumeContactsModule>()
    .AddApplicationCommandModule<NatsumeHqUserCommandModule>();

host.Services.UseScheduler(scheduler =>
    scheduler
        .Schedule<BondUpInvocable>()
        .Hourly()
);

await host.RunAsync();