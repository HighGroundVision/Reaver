using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.Abilities
{
    public class AbilityStat
    {
        [JsonProperty("abilityId")]
        public int AbilityId { get; set; }

        [JsonProperty("avgPickPosition")]
        public double AvgPickPosition { get; set; }

        [JsonProperty("pickRate")]
        public double PickRate { get; set; }

        [JsonProperty("winrate")]
        public double Winrate { get; set; }

        [JsonIgnore()]
        public double Value { get;  set; }

        [JsonIgnore()]
        public int HeroId { get;  set; }

        [JsonIgnore()]
        public string? Image { get;  set; }

        [JsonIgnore()]
        public string? Name { get;  set; }

        [JsonIgnore()]
        public string? Description { get;  set; }

        [JsonIgnore()]
        public string? Keywords { get;  set; }

        [JsonIgnore()]
        public string? Notes { get;  set; }
    }

    public class Data
    {
        [JsonProperty("abilityStats")]
        public List<AbilityStat> AbilityStats { get; set; } = new List<AbilityStat>();

        [JsonProperty("abilityValuations")]
        public Dictionary<int, double> AbilityValuations { get; set; } = new Dictionary<int, double>();
    }

    public class Root
    {
        [JsonProperty("data")]
        public Data? Data { get; set; }
    }
}
