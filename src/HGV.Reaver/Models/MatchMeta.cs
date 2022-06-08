using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.MatchData
{
    public class MatchMetaStatus
    {
        public const string OK = "ok";
        public const string Error = "error-not-found";
        public const string Parsed = "parsed";
    }

    public class MatchMetaStates
    {
        public const string Error = "Error";
        public const string MatchNotFound = "MatchNotFound";
        public const string ReplayNotFound = "ReplayNotFound";
        public const string NotParsed = "NotParsed";
        public const string Parsed = "Parsed";
    }

    public class MatchMeta
    {
        [JsonProperty("status")]
        public string? Status { get; set; }

        [JsonProperty("date_created")]
        public DateTime? DateCreated { get; set; }

        [JsonProperty("game_start")]
        public DateTime? GameStart { get; set; }

        [JsonProperty("state")]
        public string? State { get; set; }

        [JsonProperty("replay_url_acquired")]
        public bool? ReplayUrlAcquired { get; set; }
    }
}
