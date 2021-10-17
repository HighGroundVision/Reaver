using Azure.Data.Tables;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IAccountService
    {
        Task AddLink(UserEntity user);
        Task RemoveLink(ulong GuildId, ulong UserId);
        Task<UserEntity> GetLinkedAccount(ulong GuildId, ulong UserId);
    }

    public class AccountService : IAccountService
    {
        private const string TABLE_NAME = "hgvReaverUsers";
        private readonly string connectionString;

        public AccountService(IOptions<ReaverSettings> settings)
        {
            this.connectionString = settings.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));
        }

        public async Task AddLink(UserEntity user)
        {
            var client = await GetTableClient();
            await client.UpsertEntityAsync(user, TableUpdateMode.Replace);
        }

        public async Task RemoveLink(ulong GuildId, ulong UserId)
        {
            try
            {
                var client = await GetTableClient();
                await client.DeleteEntityAsync(GuildId.ToString(), UserId.ToString());
            }
            catch (Exception)
            {
                // TODO: Dose this matter...?
            }
        }

        public async Task<UserEntity> GetLinkedAccount(ulong GuildId, ulong UserId)
        {
            try
            {
                var client = await GetTableClient();
                var reponse = await client.GetEntityAsync<UserEntity>(GuildId.ToString(), UserId.ToString());
                return reponse.Value;
            }
            catch (Azure.RequestFailedException)
            {
                throw new AccountNotLinkedException();
            }
        }

        private async Task<TableClient> GetTableClient()
        {
            var client = new TableServiceClient(this.connectionString);
            var table = client.GetTableClient(TABLE_NAME);
            await table.CreateIfNotExistsAsync();
            return table;
        }
    }
}
