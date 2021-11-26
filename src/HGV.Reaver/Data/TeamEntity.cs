using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Data
{
    public class TeamEntity
    {
        public Guid Id { get; set; }
        public ulong GuidId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public string ETag { get; set; } = string.Empty;
    }
}
