using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace HGV.Reaver.Models
{
    public class ReaverSettings
    {
        public string? BaseURL { get; set; }
        public string? WindrunUrl { get; set; }
        public string? DiscordClientId { get; set; }
        public string? DiscordClientSecret { get; set; }
        public string? DiscordBotToken { get; set; }
        public string? SteamKey { get; set; }
        public string? SteamUsername { get; set; }
        public string? SteamPassword { get; set; }
        public string? StorageConnectionString { get; set; }
        public string? CosmosConnectionString { get; set; }
        public string? BrowserlessToken { get; set; }
        public string? ShotstackUrl { get; set; }
        public string? ShotstackToken { get; set; }
    }
}
