using HGV.Reaver.Models;
using HGV.Reaver.Models.MatchData;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IMatchServices
    {
        Task<MatchMeta> GetMeta(long matchId);
        Task<MatchData> GetMatch(long matchId);
    }

    public class MatchServices : IMatchServices
    {
        private readonly string windrunUrl;
        private readonly HttpClient httpClient;

        public MatchServices(IOptions<ReaverSettings> settings, HttpClient client)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.httpClient = client;
        }

        public async Task<MatchMeta> GetMeta(long matchId)
        {
            var json = await this.httpClient.GetStringAsync($"api/matches/{matchId}/meta");
            var model = JsonConvert.DeserializeObject<MatchMeta>(json);
            return model ?? throw new NullReferenceException("IMatchServices::GetMeta::DeserializeObject::MatchMeta");
        }

        public async Task<MatchData> GetMatch(long matchId)
        {
            var json = await this.httpClient.GetStringAsync($"api/matches/{matchId}");
            var model = JsonConvert.DeserializeObject<MatchReponse>(json);
            return model?.Data ?? throw new NullReferenceException("IMatchServices::GetMatch::DeserializeObject::MatchMeta"); ;
        }
    }
}
