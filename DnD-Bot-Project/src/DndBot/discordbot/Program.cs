using Discord;
using Discord;
using Discord.Commands;
using Discord.Commands;
using Discord.WebSocket;
using Discord.WebSocket;
using DndBot.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace DndBot
{
    public class Program
    {
        private static DiscordSocketClient _client;
        private static CommandService _commands;
        private static IServiceProvider _serviceProvider;

        static void Main(string[] args) => RunBotAsync().GetAwaiter().GetResult();

        static async Task RunBotAsync()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            }));
            services.AddSingleton<CommandService>();
            services.AddHttpClient("DndApi", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7291/");
            });

            _serviceProvider = services.BuildServiceProvider();
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
            _commands = _serviceProvider.GetRequiredService<CommandService>();

            _client.Log += Log;
            _client.MessageReceived += MessageReceivedAsync;

            // Регистрируем модули
            await _commands.AddModuleAsync<CharacterModule>(_serviceProvider);
            await _commands.AddModuleAsync<ClassModule>(_serviceProvider);
            await _commands.AddModuleAsync<BranchModule>(_serviceProvider);
            await _commands.AddModuleAsync<AbilityModule>(_serviceProvider);
            await _commands.AddModuleAsync<StatModule>(_serviceProvider);
            await _commands.AddModuleAsync<PoolModule>(_serviceProvider);
            await _commands.AddModuleAsync<ItemModule>(_serviceProvider);

            string token = "token_here";
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg) { Console.WriteLine(msg); return Task.CompletedTask; }

        private static async Task MessageReceivedAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message) return;
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);

            // Запускаем выполнение команды в фоновом потоке, чтобы не блокировать шлюз
            _ = Task.Run(async () =>
            {
                await _commands.ExecuteAsync(context, argPos, _serviceProvider);
            });
        }
    }
}