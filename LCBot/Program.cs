using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using LCBot.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", false, true);

builder.Services.AddDiscordHost((config, _) =>
{
    config.SocketConfig = new DiscordSocketConfig
    {
        LogLevel = LogSeverity.Verbose,
        AlwaysDownloadUsers = true,
        MessageCacheSize = 200,
        GatewayIntents = GatewayIntents.All
    };

    config.Token = builder.Configuration["Token"]!;
});


builder.Services.AddCommandService((config, _) =>
{
    config.DefaultRunMode = RunMode.Async;
    config.CaseSensitiveCommands = false;
});

builder.Services.AddInteractionService((config, _) =>
{
    config.LogLevel = LogSeverity.Info;
    config.UseCompiledLambda = true;
});

builder.Services.AddHostedService<CommandHandler>();

builder.Services.AddSingleton<LeagueModule>();

var host = builder.Build();

await host.RunAsync();