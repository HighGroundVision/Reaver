using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace HGV.Reaver.Models
{
    public class ReaverSettings
    {
        public string BaseURL { get; set; }
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string DiscordBotToken { get; set; }
        public string SteamKey { get; set; }
        public string StorageConnectionString { get; set; }
    }
}
