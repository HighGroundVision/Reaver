using HGV.Reaver.Models.Meta;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IMatchServices
    {
        Task<MatchMeta> GetMeta(long matchId);
    }

    public class MatchServices : IMatchServices
    {
        private readonly HttpClient httpClient;

        public MatchServices(HttpClient client)
        {
            this.httpClient = client;
        }

        public async Task<MatchMeta> GetMeta(long matchId)
        {
            var json = await this.httpClient.GetStringAsync($"https://ad.datdota.com/api/matches/{matchId}/meta");
            var model = JsonConvert.DeserializeObject<MatchMeta>(json);
            return model;
        }
    }
}
