using Azure.Data.Tables;
using DSharpPlus;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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
        private const string TABLE_NAME = "userLUT";
        
        private readonly TableClient tableClient;

        public AccountService(IOptions<ReaverSettings> settings)
        {
            var connectionString = settings.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));
            this.tableClient = new TableClient(connectionString, TABLE_NAME);
            this.tableClient.CreateIfNotExists();
        }

        public async Task AddLink(UserEntity user)
        {
            await this.tableClient.UpsertEntityAsync(user, TableUpdateMode.Replace);
        }

        public async Task RemoveLink(ulong GuildId, ulong UserId)
        {
            try
            {
                await this.tableClient.DeleteEntityAsync(GuildId.ToString(), UserId.ToString());
            }
            catch (Exception)
            {
            }
        }

        public async Task<UserEntity> GetLinkedAccount(ulong GuildId, ulong UserId)
        {
            try
            {
                var reponse = await this.tableClient.GetEntityAsync<UserEntity>(GuildId.ToString(), UserId.ToString());
                return reponse.Value;
            }
            catch (Azure.RequestFailedException)
            {
                throw new AccountNotLinkedException();
            }
        }

    }
}
