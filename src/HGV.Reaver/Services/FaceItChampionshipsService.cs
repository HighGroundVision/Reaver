using HGV.Reaver.Models;
using HGV.Reaver.Models.FaceIt;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IFaceItChampionshipsService
    {
        Task<TeamSummary> GetTeam(string id);
        Task<Player> GetPlayer(string id);
    }

    public class FaceItChampionshipsService : IFaceItChampionshipsService
    {
        private readonly HttpClient client;

        public FaceItChampionshipsService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<TeamSummary> GetTeam(string id)
        {
            var json = await this.client.GetStringAsync($"https://open.faceit.com/data/v4/teams/{id}");
            return JsonConvert.DeserializeObject<TeamSummary>(json) ?? throw new NullReferenceException("IFaceItChampionshipsService::GetTeam::DeserializeObject::TeamSummary.");
        }

        public async Task<Player> GetPlayer(string id)
        {
            var json = await this.client.GetStringAsync($"https://open.faceit.com/data/v4/players/{id}");
            return JsonConvert.DeserializeObject<Player>(json) ?? throw new NullReferenceException("IFaceItChampionshipsService::GetPlayer::DeserializeObject::Player.");
        }
    }
}
