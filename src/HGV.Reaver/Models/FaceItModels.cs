using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.FaceIt
{
    public class Platforms
    {
        [JsonProperty("steam")]
        public string? Steam { get; set; }
    }

    public class Csgo
    {
        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("game_player_id")]
        public string? GamePlayerId { get; set; }

        [JsonProperty("skill_level")]
        public int? SkillLevel { get; set; }

        [JsonProperty("faceit_elo")]
        public int? FaceitElo { get; set; }

        [JsonProperty("game_player_name")]
        public string? GamePlayerName { get; set; }

        [JsonProperty("skill_level_label")]
        public string? SkillLevelLabel { get; set; }

        [JsonProperty("game_profile_id")]
        public string? GameProfileId { get; set; }
    }

    public class Dota2
    {
        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("game_player_id")]
        public string? GamePlayerId { get; set; }

        [JsonProperty("skill_level")]
        public int? SkillLevel { get; set; }

        [JsonProperty("faceit_elo")]
        public int? FaceitElo { get; set; }

        [JsonProperty("game_player_name")]
        public string? GamePlayerName { get; set; }

        [JsonProperty("skill_level_label")]
        public string? SkillLevelLabel { get; set; }

        [JsonProperty("game_profile_id")]
        public string? GameProfileId { get; set; }
    }

    public class Games
    {
        [JsonProperty("csgo")]
        public Csgo? Csgo { get; set; }

        [JsonProperty("dota2")]
        public Dota2? Dota2 { get; set; }
    }

    public class Settings
    {
        [JsonProperty("language")]
        public string? Language { get; set; }
    }

    public class Player
    {
        [JsonProperty("player_id")]
        public string? PlayerId { get; set; }

        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("cover_image")]
        public string? CoverImage { get; set; }

        [JsonProperty("platforms")]
        public Platforms? Platforms { get; set; }

        [JsonProperty("games")]
        public Games? Games { get; set; }

        [JsonProperty("settings")]
        public Settings? Settings { get; set; }

        [JsonProperty("friends_ids")]
        public List<string> FriendsIds { get; set; } = new List<string>();

        [JsonProperty("new_steam_id")]
        public string? NewSteamId { get; set; }

        [JsonProperty("steam_id_64")]
        public string? SteamId64 { get; set; }

        [JsonProperty("steam_nickname")]
        public string? SteamNickname { get; set; }

        [JsonProperty("memberships")]
        public List<string> Memberships { get; set; } = new List<string>();

        [JsonProperty("faceit_url")]
        public string? FaceitUrl { get; set; }

        [JsonProperty("membership_type")]
        public string? MembershipType { get; set; }

        [JsonProperty("cover_featured_image")]
        public string? CoverFeaturedImage { get; set; }

        public SteamID GetSteamId()
        {
            if (string.IsNullOrWhiteSpace(this.SteamId64))
            {
                var accountId = this.Games?.Dota2?.GamePlayerId ?? string.Empty;
                if (string.IsNullOrWhiteSpace(accountId))
                {
                    throw new UserFriendlyException("FaceIt Player missing SteamId");
                }
                else
                {
                    var id = new SteamID();
                    id.SetFromSteam3String(accountId);
                    return id;
                }
            }
            else
            {
                if (ulong.TryParse(this.SteamId64, out ulong id))
                {
                    return new SteamID(id);
                }
                else
                {
                    throw new UserFriendlyException("FaceIt Player missing SteamId");
                }
            }
        }
    }

    public class TeamSummary
    {
        [JsonProperty("team_id")]
        public string? TeamId { get; set; }

        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("cover_image")]
        public string? CoverImage { get; set; }

        [JsonProperty("game")]
        public string? Game { get; set; }

        [JsonProperty("team_type")]
        public string? TeamType { get; set; }

        [JsonProperty("members")]
        public List<Member> Members { get; set; } = new List<Member>();

        [JsonProperty("leader")]
        public string? Leader { get; set; }

        [JsonProperty("website")]
        public string? Website { get; set; }

        [JsonProperty("facebook")]
        public string? Facebook { get; set; }

        [JsonProperty("twitter")]
        public string? Twitter { get; set; }

        [JsonProperty("youtube")]
        public string? Youtube { get; set; }

        [JsonProperty("chat_room_id")]
        public string? ChatRoomId { get; set; }

        [JsonProperty("faceit_url")]
        public string? FaceitUrl { get; set; }
    }

    public class Member
    {
        [JsonProperty("user_id")]
        public string? UserId { get; set; }

        [JsonProperty("nickname")]
        public string? Nickname { get; set; }

        [JsonProperty("avatar")]
        public string? Avatar { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("skill_level")]
        public int? SkillLevel { get; set; }

        [JsonProperty("faceit_url")]
        public string? FaceitUrl { get; set; }
    }
}
