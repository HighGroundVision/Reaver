using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Data
{
    public class UserLinkEntity
    {
        public Guid Id { get; set; }
        public ulong GuidId { get; set; }

        public ulong UserId { get; set; }
        public ulong SteamId { get; set; }
        public uint DotaId => (uint)(SteamId - 76561197960265728L);
        public string Email { get; set; }

        public string ETag { get; set; }
    }
}
