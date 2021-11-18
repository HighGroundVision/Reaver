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

        public string Name { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string Color { get; set; }

        public string ETag { get; set; }
    }
}
