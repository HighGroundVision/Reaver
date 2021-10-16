using HGV.Reaver.Bot.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot.Services
{
    public interface IProfileService
    {
        Task<Profile> GetProfile(ulong accountId);
    }

    public class ProfileService : IProfileService
    {
        private readonly HttpClient _httpClient;

        public ProfileService(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<Profile> GetProfile(ulong steamId)
        {
            var json = await _httpClient.GetStringAsync($"https://ad.datdota.com/api/players/{steamId}");
            var model = JsonConvert.DeserializeObject<Root>(json);
            return model.Data;
        }
    }
}
