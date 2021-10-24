using Azure.Data.Tables;
using DSharpPlus;
using DSharpPlus.Entities;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public enum Roles
    {
        Everyone = 0,
        Moderators = 1,
        Members = 2,
    }

    public interface IRolesService
    {
        Task<DiscordRole> GetRole(DiscordClient discordClient, ulong id, Roles r);
    }

    public class RolesService : IRolesService
    {
        private const string TABLE_NAME = "rolesLUT";
        private readonly string connectionString;

        public RolesService(IOptions<ReaverSettings> settings)
        {
            this.connectionString = settings.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var client = new TableClient(this.connectionString, TABLE_NAME);
            client.CreateIfNotExists();
        }


        public async Task<DiscordRole> GetRole(DiscordClient discordClient, ulong id, Roles r)
        {
            var tableClient = new TableClient(this.connectionString, TABLE_NAME);
            var reponse = await tableClient.GetEntityAsync<RoleEntity>(id.ToString(), r.ToString("d"));
            var roleId = (ulong)reponse.Value.RoleId;

            var guild = await discordClient.GetGuildAsync(id);
            var role = guild.Roles[roleId];
            return role;
        }

    }
}
