using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot
{
    internal class DiscordLifetimeHost : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly string _token;
        private readonly DiscordSocketClient _discordClient;
        private readonly HttpClient _httpClient;

        public DiscordLifetimeHost(ILogger<DiscordLifetimeHost> logger, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _token = configuration["DiscordToken"];
            _logger = logger;
            _httpClient = clientFactory.CreateClient();
            _discordClient = new DiscordSocketClient();
            _discordClient.Log += LogAsync;
            _discordClient.Ready += ReadyAsync;
            _discordClient.MessageReceived += MessageReceivedAsync;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _discordClient.LoginAsync(TokenType.Bot, _token);
            await _discordClient.StartAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.StopAsync();
            await base.StopAsync(cancellationToken);
        }

        private Task LogAsync(LogMessage log)
        {
            _logger.LogInformation(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
             _logger.LogInformation($"{_discordClient.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        // This is not the recommended way to write a bot - consider reading over the Commands Framework sample.
        private Task MessageReceivedAsync(SocketMessage message)
        {
            Task.Run(() => HandleMessage(message));
            return Task.CompletedTask;
        }

        private async Task HandleMessage(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _discordClient.CurrentUser.Id)
                return;

            var parameters = message.Content.Split(" ");
            
            // The bot should only handle commands with the '!hgv' perfix.
            var prefix = parameters?[0]?.Trim() ?? string.Empty;
            if(prefix != "!hgv")
                return;

            var command = parameters?[1]?.Trim() ?? string.Empty;
            switch (command)
            {
                case "image":
                    await ImageCommands(message, parameters[2..^0]);
                    break;
                default:
                    await FailedParseParameters(message);
                    return;
            }
        }

        private static async Task FailedParseParameters(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Failed to parse parameters, check the command and try again.");
        }

        private static async Task<ulong> DelayGetImage(SocketMessage message)
        {
            var msg = await message.Channel.SendMessageAsync($"It is taking longer then expected to generate the image...");
            return msg.Id;
        }

        private async Task ImageCommands(SocketMessage message, string[] parameters)
        {
            if(parameters.Length != 2)
            {
                await FailedParseParameters(message);
                return;
            }

            var commmmand = parameters?[0]?.Trim() ?? string.Empty;
            var option = parameters?[1]?.Trim() ?? string.Empty;

            if (Uri.IsWellFormedUriString(option, UriKind.Absolute))
                option = option.Substring(option.LastIndexOf('/') + 1);

            if (long.TryParse(option, out long id) == false)
            {
                await FailedParseParameters(message);
                return;
            }

            switch (commmmand)
            {
                case "summary":
                    await HandleSummary(message, id);
                    break;
                case "players":
                    await HandlePlayers(message, id);
                    break;
                case "draft":
                    await HandleDraft(message, id);
                    break;
                default:
                    break;
            }
        }

        private async Task HandleSummary(SocketMessage message, long id)
        {
            ulong? delayMessageId = null;

            var policy = Policy<Stream>
                .Handle<Exception>()
                .WaitAndRetryAsync(6, (i) => TimeSpan.FromSeconds(i * 10) , onRetry: async (r, ts, i, ctx) => { 
                    if(i == 3)
                    {
                        delayMessageId =  await DelayGetImage(message);
                    }
                });

            try
            {
                var url = $"https://quarterstaff.azurewebsites.net/api/images/{id}/summary";
                var stream = await policy.ExecuteAsync(async () => await _httpClient.GetStreamAsync(url));
                await message.Channel.SendFileAsync(stream, $"summary.{id}.png", text: $"Summary for Match {id}");

                if(delayMessageId.HasValue)
                {
                    await message.Channel.DeleteMessageAsync(delayMessageId.Value);
                }
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Failed to generate image of Summary for {id}");
            }
        }

        

        private async Task HandlePlayers(SocketMessage message, long id)
        {
             ulong? delayMessageId = null;

            var policy = Policy<Stream>
                .Handle<Exception>()
                .WaitAndRetryAsync(6, (i) => TimeSpan.FromSeconds(i * 10) , onRetry: async (r, ts, i, ctx) => { 
                    if(i == 3)
                    {
                        delayMessageId =  await DelayGetImage(message);
                    }
                });

            try
            {
                var url = $"https://quarterstaff.azurewebsites.net/api/images/{id}/players";
                var stream = await policy.ExecuteAsync(async () => await _httpClient.GetStreamAsync(url));
                await message.Channel.SendFileAsync(stream, $"players.{id}.png", text: $"Players for Match {id}");

                if(delayMessageId.HasValue)
                {
                    await message.Channel.DeleteMessageAsync(delayMessageId.Value);
                }
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Failed to generate image of Players for {id}");
            }
        }

        private async Task HandleDraft(SocketMessage message, long id)
        {
             ulong? delayMessageId = null;

            var policy = Policy<Stream>
                .Handle<Exception>()
                .WaitAndRetryAsync(3, (i) => TimeSpan.FromSeconds(i * 30) , onRetry: async (r, ts, i, ctx) => { 
                    if(i == 1)
                    {
                        delayMessageId =  await DelayGetImage(message);
                    }
                });

            try
            {
                var url = $"https://quarterstaff.azurewebsites.net/api/images/{id}/draft";
                var stream = await policy.ExecuteAsync(async () => await _httpClient.GetStreamAsync(url));
                await message.Channel.SendFileAsync(stream, $"draft.{id}.gif", text: $"Draft for Match {id}");

                if(delayMessageId.HasValue)
                {
                    await message.Channel.DeleteMessageAsync(delayMessageId.Value);
                }
            }
            catch (Exception ex)
            {
                await message.Channel.SendMessageAsync($"Failed to generate image of Draft for {id}");
            }
        }

    }
}
