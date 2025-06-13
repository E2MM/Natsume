using Coravel;
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

var sqliteConnection = builder.Configuration.GetSection("SQLite")["ConnectionString"];
if (sqliteConnection is null or "") throw new ApplicationException("Invalid SQLite Connection String");

builder.Services
    .AddDbContext<NatsumeDbContext>(options =>
        options.UseSqlite(sqliteConnection))//, ServiceLifetime.Singleton)
    .AddSingleton<NatsumeDbService>();


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
    .AddScoped<IOpenAiService, OpenAiService>(_ => new OpenAiService(openAiApiKey))
    .AddScoped<NatsumeAi>()
    .AddTransient<BondUpInvocable>();

var host = builder
    .Build()
    .UseGatewayEventHandlers()
    .AddApplicationCommandModule<NatsumeCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule>()
    .AddApplicationCommandModule<NatsumeHqSlashCommandModule.NatsumeContactsModule>()
    .AddApplicationCommandModule<NatsumeHqUserCommandModule>()
    .AddApplicationCommandModule<NatsumeGoogleMeetCommandModule>();

// Esegui la migrazione del database all'avvio
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NatsumeDbContext>();
    await db.Database.MigrateAsync();
}


host.Services.UseScheduler(scheduler =>
    scheduler
        .Schedule<BondUpInvocable>()
        .Hourly()
);


await host.RunAsync();