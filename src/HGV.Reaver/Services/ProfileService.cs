using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IProfileService
    {
        Task<HGV.Reaver.Models.DotaProfile.DotaProfile> GetDotaProfile(ulong accountId);
        Task<HGV.Reaver.Models.SteamProfile.SteamProfile> GetSteamProfile(ulong steamId);
    }

    public class ProfileService : IProfileService
    {
        private readonly HttpClient httpClient;
        private readonly string steamKey;

        public ProfileService(HttpClient client, IOptions<Models.ReaverSettings> settings)
        {
            this.httpClient = client;
            this.steamKey = settings?.Value?.SteamKey;
        }

        public async Task<HGV.Reaver.Models.DotaProfile.DotaProfile> GetDotaProfile(ulong steamId)
        {
            var json = await this.httpClient.GetStringAsync($"https://ad.datdota.com/api/players/{steamId}");
            var model = JsonConvert.DeserializeObject<HGV.Reaver.Models.DotaProfile.Root>(json);
            if (model.Data.AccountId is null)
                return null;
            else 
                return model.Data;
        }

        public async Task<HGV.Reaver.Models.SteamProfile.SteamProfile> GetSteamProfile(ulong steamId)
        {
            var url = string.Format("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}", this.steamKey, steamId);
            var json = await this.httpClient.GetStringAsync(url);
            var model = JsonConvert.DeserializeObject<HGV.Reaver.Models.SteamProfile.Root>(json);
            return model.Response.Profiles.FirstOrDefault();
        }
    }
}
