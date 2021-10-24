using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Models.SteamProfile
{
	public class SteamProfile
	{
		[Newtonsoft.Json.JsonProperty("steamid")]
		public ulong SteamId { get; set; }

		[Newtonsoft.Json.JsonProperty("personaname")]
		public string Persona { get; set; }

		[Newtonsoft.Json.JsonProperty("realname")]
		public string Name { get; set; }

		[Newtonsoft.Json.JsonProperty("profileurl")]
		public string ProfileUrl { get; set; }

		[Newtonsoft.Json.JsonProperty("avatar")]
		public string AvatarSmall { get; set; }

		[Newtonsoft.Json.JsonProperty("avatarmedium")]
		public string AvatarMedium { get; set; }

		[Newtonsoft.Json.JsonProperty("avatarfull")]
		public string AvatarLarge { get; set; }

		[Newtonsoft.Json.JsonProperty("loccountrycode")]
		public string CountryCode { get; set; }

		[Newtonsoft.Json.JsonProperty("locstatecode")]
		public string StateCode { get; set; }
	}

	public class Root
	{
		[Newtonsoft.Json.JsonProperty("response")]
		public PlayerSummariesReponse Response { get; set; }
	}

	public class PlayerSummariesReponse
	{
		[Newtonsoft.Json.JsonProperty("players")]
		public List<SteamProfile> Profiles { get; set; }
	}
}
