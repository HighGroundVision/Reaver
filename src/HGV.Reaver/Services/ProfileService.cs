using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IProfileService
    {
        Task<HGV.Reaver.Models.DotaProfile.DotaProfile?> GetDotaProfile(ulong accountId);
        Task<HGV.Reaver.Models.SteamProfile.SteamProfile?> GetSteamProfile(ulong steamId);
    }

    public class ProfileService : IProfileService
    {
        private readonly string windrunUrl;
        private readonly HttpClient httpClient;
        private readonly string steamKey;

        public ProfileService(HttpClient client, IOptions<ReaverSettings> settings)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.steamKey = settings?.Value?.SteamKey ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.SteamKey));
            this.httpClient = client;
        }

        public async Task<HGV.Reaver.Models.DotaProfile.DotaProfile?> GetDotaProfile(ulong steamId)
        {
            var json = await this.httpClient.GetStringAsync($"api/players/{steamId}");
            var model = JsonConvert.DeserializeObject<HGV.Reaver.Models.DotaProfile.Root>(json);
            if (model?.Data?.AccountId is null)
                return null;
            else 
                return model?.Data;
        }

        public async Task<HGV.Reaver.Models.SteamProfile.SteamProfile?> GetSteamProfile(ulong steamId)
        {
            var url = string.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}", this.steamKey, steamId);
            var json = await this.httpClient.GetStringAsync(url);
            var model = JsonConvert.DeserializeObject<HGV.Reaver.Models.SteamProfile.Root>(json) ?? throw new NullReferenceException("ProfileService::GetSteamProfile::DeserializeObject");
            return model.Response?.Profiles.FirstOrDefault();
        }
    }
}
