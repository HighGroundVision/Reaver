using HGV.Reaver.Models.Profile;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IProfileService
    {
        Task<Profile> GetProfile(string accountId);
    }

    public class ProfileService : IProfileService
    {
        private readonly HttpClient httpClient;

        public ProfileService(HttpClient client)
        {
            this.httpClient = client;
        }

        public async Task<Profile> GetProfile(string steamId)
        {
            var json = await this.httpClient.GetStringAsync($"https://ad.datdota.com/api/players/{steamId}");
            var model = JsonConvert.DeserializeObject<Root>(json);
            return model.Data;
        }
    }
}
