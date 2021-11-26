using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.DotaProfile
{
    public class WinLossData
    {
        [JsonProperty("losses")]
        public int? Losses { get; set; }

        [JsonProperty("winrate")]
        public double? Winrate { get; set; }

        [JsonProperty("wins")]
        public int? Wins { get; set; }
    }

    public class DotaProfile
    {
        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("lastMatch")]
        public DateTime? LastMatch { get; set; }

        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("overallRank")]
        public int? OverallRank { get; set; }

        [JsonProperty("percentile")]
        public double? Percentile { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("regionalRank")]
        public int? RegionalRank { get; set; }

        [JsonProperty("steamId")]
        public int? AccountId { get; set; }

        [JsonProperty("winLoss")]
        public WinLossData? WinLoss { get; set; }
    }

    public class Root
    {
        [JsonProperty("data")]
        public DotaProfile? Data { get; set; }
    }
}
