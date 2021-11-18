using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Data
{
    public class RoleLinkEntity
    {
        public Guid Id { get; set; }
        public ulong GuidId { get; set; }

        public ulong MessageId { get; set; }
        public string EmojiName { get; set; }
        public ulong RoleId { get; set; }

        public string ETag { get; set; }
    }
}
