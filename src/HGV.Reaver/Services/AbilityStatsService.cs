using HGV.Basilius;
using HGV.Basilius.Client;
using HGV.Reaver.Models.Abilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IAbilityStatsService
    {
        Task<AbilityStat> GetAbility(int id);
        Task<AbilityStat> GetAbility(string name);
    }

    public class AbilityStatsService : IAbilityStatsService
    {
        private readonly HttpClient httpClient;
        private readonly IMetaClient metaClient;

        public AbilityStatsService(HttpClient client, IMetaClient metaClient)
        {
            this.httpClient = client;
            this.metaClient = metaClient;
        }

        private async Task<List<AbilityStat>> GetAbilities()
        {
            var abilities = this.metaClient.GetAbilities();

            var json = await this.httpClient.GetStringAsync($"https://ad.datdota.com/api/abilities");
            var model = JsonConvert.DeserializeObject<Root>(json);

            var collection = model.Data.AbilityStats
                .Join(model.Data.AbilityValuations, _ => _.AbilityId, _ => _.Key, (lhs, rhs) =>
                {
                    lhs.Value = rhs.Value;
                    return lhs;
                })
                .Join(abilities, _ => _.AbilityId, _ => _.Id, (lhs, rhs) =>
                {
                    lhs.HeroId = rhs.HeroId;
                    lhs.Image = rhs.Image;
                    lhs.Name = rhs.Name;
                    lhs.Description = rhs.Description;
                    lhs.Notes = rhs.AbilityDraftNote;
                    lhs.Keywords = string.Join(", ", rhs.Keywords);
                    return lhs;
                })
                .ToList();

            return collection;
        }

        public async Task<AbilityStat> GetAbility(int id)
        {
            var collection = await this.GetAbilities();
            var data = collection.Find(_ => _.AbilityId == id);
            return data;
        }

        public async Task<AbilityStat> GetAbility(string name)
        {
            var collection = await this.GetAbilities();
            var data = collection.Find(_ => _.Name == name);
            return data;
        }
    }
}
