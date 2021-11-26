using HGV.Basilius;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using HGV.Reaver.Models.Abilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IAbilityStatsService
    {
        Task<AbilityStat> GetAbility(string id);
    }

    public class AbilityStatsService : IAbilityStatsService
    {
        private readonly string windrunUrl;
        private readonly HttpClient httpClient;
        private readonly IMetaClient metaClient;

        public AbilityStatsService(IOptions<ReaverSettings> settings, HttpClient client, IMetaClient metaClient)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.httpClient = client;
            this.metaClient = metaClient;
        }

        private async Task<List<AbilityStat>> GetAbilities()
        {
            var abilities = this.metaClient.GetAbilities();

            var json = await this.httpClient.GetStringAsync($"{windrunUrl}/api/abilities");
            var model = JsonConvert.DeserializeObject<Root>(json);

            if (model?.Data is null)
                throw new NullReferenceException("IAbilityStatsService::GetAbilities::Model");

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

        public async Task<AbilityStat> GetAbility(string id)
        {
            var abilityId = int.Parse(id);
            var collection = await this.GetAbilities();
            var ability = collection.FirstOrDefault(_ => _.AbilityId == abilityId);
            if (ability is null)
                throw new UserFriendlyException($"Unable to find ability {id}");

            return ability;
        }
    }
}