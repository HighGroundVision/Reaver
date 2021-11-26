using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.MatchData
{
    public class WinLoss
    {
        [JsonProperty("losses")]
        public int Losses { get; set; }

        [JsonProperty("winrate")]
        public double Winrate { get; set; }

        [JsonProperty("wins")]
        public int Wins { get; set; }
    }

    public class AbilityIdentity
    {
        [JsonProperty("abilityId")]
        public int? AbilityId { get; set; }
    }

    public class Pick : AbilityIdentity
    {
        [JsonProperty("pickOrder")]
        public int? PickOrder { get; set; }
    }

    public class Team
    {
        [JsonProperty("abilities")]
        public List<int> Abilities { get; set; } = new List<int>();

        [JsonProperty("assists")]
        public int? Assists { get; set; }

        [JsonProperty("deaths")]
        public int? Deaths { get; set; }

        [JsonProperty("gpm")]
        public int? Gpm { get; set; }

        [JsonProperty("hero")]
        public int? Hero { get; set; }

        [JsonProperty("heroDamage")]
        public int? HeroDamage { get; set; }

        [JsonProperty("heroHealing")]
        public int? HeroHealing { get; set; }

        [JsonProperty("items")]
        public List<int> Items { get; set; } = new List<int>();

        [JsonProperty("kills")]
        public int? Kills { get; set; }

        [JsonProperty("lastHits")]
        public int? LastHits { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("rating")]
        public double? Rating { get; set; }

        [JsonProperty("steamId")]
        public int? SteamId { get; set; }

        [JsonProperty("topX")]
        public int? TopX { get; set; }

        [JsonProperty("winLoss")]
        public WinLoss? WinLoss { get; set; }

        [JsonProperty("xpm")]
        public int? Xpm { get; set; }
    }

    public class MatchData
    {
        [JsonProperty("dire")]
        public List<Team> Dire { get; set; } = new List<Team>();

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("gameStart")]
        public DateTime? GameStart { get; set; }

        [JsonProperty("ignoredSpells")]
        public List<AbilityIdentity> IgnoredAbilities { get; set; } = new List<AbilityIdentity>();

        [JsonProperty("matchId")]
        public long MatchId { get; set; }

        [JsonProperty("picks")]
        public List<Pick> Picks { get; set; } = new List<Pick>();

        [JsonProperty("radiant")]
        public List<Team> Radiant { get; set; } = new List<Team>();

        [JsonProperty("radiantWin")]
        public bool? RadiantWin { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }
    }

    public class MatchReponse
    {
        [JsonProperty("data")]
        public MatchData? Data { get; set; }
    }
}
