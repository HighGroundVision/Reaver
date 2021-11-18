using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Services;
using System;
using System.Threading.Tasks;


namespace HGV.Reaver.Commands
{
    public class ProfileContextMenu : ApplicationCommandModule
    {
        private const string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";

        private readonly IAccountService accountService;
        private readonly IProfileService profileService;
        private readonly IRanksImageService ranksImageService;

        public ProfileContextMenu(IAccountService accountService, IProfileService profileService, IRanksImageService ranksImageService)
        {
            this.accountService = accountService;
            this.profileService = profileService;
            this.ranksImageService = ranksImageService;
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Refresh", false)]
        public async Task Refresh(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var account = await this.accountService.Get(ctx.Member.Guild.Id, ctx.TargetMember.Id);
            var dota = await this.profileService.GetDotaProfile(account.SteamId);
            var steam = await this.profileService.GetSteamProfile(account.SteamId);

            var nickname = steam?.Persona ?? dota.Nickname;
            if (nickname is not null)
            {
                await ctx.TargetMember.ModifyAsync(x => x.Nickname = nickname);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Profile", false)]
        public async Task GetProfile(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var user = await this.accountService.Get(ctx.Guild.Id, ctx.TargetMember.Id);
            if (user is null)
                throw new AccountNotLinkedException();

            var dota = await this.profileService.GetDotaProfile(user.SteamId);
            if (dota is null)
                throw new UserFriendlyException("Dota Account Not Found; You have not played an AD game in a while.");

            var steam = await this.profileService.GetSteamProfile(user.SteamId);
            if (steam is null)
                throw new UserFriendlyException("Steam Account Not Found; It may not be public.");

            var url = await this.ranksImageService.StorageImage(user.SteamId);


            var builder = new DiscordEmbedBuilder()
                .WithTitle(steam.Persona)
                .WithUrl(steam.ProfileUrl)
                .WithThumbnail(steam?.AvatarLarge ?? DEFAULT_IMAGE_URL)
                .WithColor(DiscordColor.Purple);

            if (url is not null)
                builder.WithImageUrl(url);

            var delta = DateTime.UtcNow - dota.LastMatch;
            if (delta.HasValue == false)
                builder.WithFooter($"Last played to long ago; Go play more Dota.");
            else if (delta.Value.Days < 1)
                builder.WithFooter($"Last played {delta.Value.Hours} hours ago.");
            else
                builder.WithFooter($"Last played {delta.Value.Days} days ago.");

            builder.AddField("ID", dota.AccountId.ToString(), false);
            builder.AddField("TOTAL", ((dota?.WinLoss?.Wins ?? 0) + (dota?.WinLoss?.Losses ?? 0)).ToString(), true);
            builder.AddField("WIN RATE", (dota.WinLoss?.Winrate ?? 0).ToString("P"), true);
            builder.AddField("RECORD", $"{(dota?.WinLoss?.Wins ?? 0)} - {(dota?.WinLoss?.Losses ?? 0)}", true);
            builder.AddField("RATING", (dota?.Rating ?? 0).ToString("F0"), false);
            builder.AddField("REGION", dota.Region.ToUpper(), true);
            builder.AddField("REGIONAL", $"#{dota.RegionalRank}", true);
            builder.AddField("WORLD", $"#{dota.OverallRank}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }
    }
}
