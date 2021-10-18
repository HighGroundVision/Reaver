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
        public double Value { get; internal set; }

        [JsonIgnore()]
        public int HeroId { get; internal set; }

        [JsonIgnore()]
        public string Image { get; internal set; }

        [JsonIgnore()]
        public string Name { get; internal set; }

        [JsonIgnore()]
        public string Description { get; internal set; }

        [JsonIgnore()]
        public string Keywords { get; internal set; }

        [JsonIgnore()]
        public string Notes { get; internal set; }
    }

    public class Data
    {
        [JsonProperty("abilityStats")]
        public List<AbilityStat> AbilityStats { get; set; }

        [JsonProperty("abilityValuations")]
        public Dictionary<int, double> AbilityValuations { get; set; }
    }

    public class Root
    {
        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}
