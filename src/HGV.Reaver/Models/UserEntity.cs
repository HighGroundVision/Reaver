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
}
