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
        private const string TABLE_NAME = "reaver";
        private readonly string connectionString;

        public AccountService(IOptions<ReaverSettings> settings)
        {
            this.connectionString = settings.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var client = new TableClient(this.connectionString, TABLE_NAME);
            client.CreateIfNotExists();
        }

        public async Task AddLink(UserEntity user)
        {
            var client = new TableClient(this.connectionString, TABLE_NAME);
            await client.UpsertEntityAsync(user, TableUpdateMode.Replace);
        }

        public async Task RemoveLink(ulong GuildId, ulong UserId)
        {
            try
            {
                var client = new TableClient(this.connectionString, TABLE_NAME);
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
                var client = new TableClient(this.connectionString, TABLE_NAME);
                var reponse = await client.GetEntityAsync<UserEntity>(GuildId.ToString(), UserId.ToString());
                return reponse.Value;
            }
            catch (Azure.RequestFailedException)
            {
                throw new AccountNotLinkedException();
            }
        }

    }
}
