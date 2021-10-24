using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models
{
    public class UserEntity : ITableEntity
    {
        public string Email { get; set; }
        public string DiscordId { get; set; }
        public string SteamId { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class User
    {
        public string Email { get; set; }
        public ulong DiscordGuidId { get; set; }
        public ulong DiscordUserId { get; set; }
        public string DiscordNickname { get; set; }
        public ulong SteamId { get; set; }
        public string SteamPersona { get; set; }
    }
}
