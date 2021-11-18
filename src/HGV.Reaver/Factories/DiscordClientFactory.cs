using DSharpPlus;
using HGV.Reaver.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Factories
{
    public interface IDiscordClientFactory
    {
        DiscordClient Client { get; }
    }

    public class DiscordClientFactory : IDiscordClientFactory
    {
        private readonly DiscordClient client;

        public DiscordClientFactory(IOptions<ReaverSettings> settings)
        {
            var token = settings?.Value?.DiscordBotToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.DiscordBotToken));

            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
            };
            this.client = new DiscordClient(config);
        }

        public DiscordClient Client => this.client;
    }
}
