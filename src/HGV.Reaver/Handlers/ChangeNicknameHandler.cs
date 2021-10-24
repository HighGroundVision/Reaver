using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Handlers
{
    public interface IChangeNicknameHandler
    {
        Task ChangeNickname(DiscordMember ctx);
        Task ChangeNickname(UserEntity entity);
    }

    public class ChangeNicknameHandler : IChangeNicknameHandler
    {
        private readonly IAccountService accountService;
        private readonly IProfileService profileService;
        private readonly DiscordClient client;

        public ChangeNicknameHandler(IAccountService accountService, IProfileService profileService, DiscordClient client)
        {
            this.accountService = accountService;
            this.profileService = profileService;
            this.client = client;
        }

        public async Task ChangeNickname(DiscordMember member)
        {
            var user = await this.accountService.GetLinkedAccount(member.Guild.Id, member.Id);
            var profile = await this.profileService.GetSteamProfile(user.SteamId);
            if (profile?.Persona is not null)
            {
                await member.ModifyAsync(x => 
                {
                    x.Nickname = profile.Persona;
                });
            }
        }

        public async Task ChangeNickname(UserEntity entity)
        {
            try
            {
                var guildId = ulong.Parse(entity.PartitionKey);
                var userId = ulong.Parse(entity.RowKey);

                var user = await this.accountService.GetLinkedAccount(guildId, userId);
                var profile = await this.profileService.GetSteamProfile(user.SteamId);
                if (profile?.Persona is not null)
                {
                    var guild = await client.GetGuildAsync(guildId);
                    var member = await guild.GetMemberAsync(userId);
                    await member.ModifyAsync(x => x.Nickname = profile.Persona);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
