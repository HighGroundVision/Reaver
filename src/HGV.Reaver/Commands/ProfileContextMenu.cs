using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Handlers;
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
        private readonly IChangeNicknameHandler handler;

        public ProfileContextMenu(IAccountService accountService, IProfileService profileService, IRanksImageService ranksImageService, IChangeNicknameHandler handler)
        {
            this.accountService = accountService;
            this.profileService = profileService;
            this.ranksImageService = ranksImageService;
            this.handler = handler;
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Refresh", false)]
        public async Task Refresh(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            await this.handler.ChangeNickname(ctx.TargetMember);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "Profile")]
        public async Task GetProfile(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var user = await this.accountService.GetLinkedAccount(ctx.Guild.Id, ctx.TargetMember.Id);
            var profile = await this.profileService.GetDotaProfile(user.SteamId);
            var url = await this.ranksImageService.StorageImage(user.SteamId);

            if (profile.AccountId is null)
                throw new UserFriendlyException("No Ability Draft Profile found.");

            var builder = new DiscordEmbedBuilder()
                .WithTitle(profile.Nickname)
                .WithUrl($"http://steamcommunity.com/profiles/{user.SteamId}/")
                .WithThumbnail(profile.Avatar ?? DEFAULT_IMAGE_URL)
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);

            var delta = DateTime.UtcNow - profile.LastMatch;
            if (delta.HasValue == false)
                builder.WithFooter($"To long ago; Go play more Dota.");
            else if (delta.Value.Days < 1)
                builder.WithFooter($"last played today!");
            else
                builder.WithFooter($"last played {delta.Value.Days} days ago...");

            builder.AddField("ID", profile.AccountId.ToString(), false);
            builder.AddField("TOTAL", ((profile?.WinLoss?.Wins ?? 0) + (profile?.WinLoss?.Losses ?? 0)).ToString(), true);
            builder.AddField("WINRATE", (profile.WinLoss?.Winrate ?? 0).ToString("P"), true);
            builder.AddField("WIN/LOSE", $"{(profile?.WinLoss?.Wins ?? 0)} - {(profile?.WinLoss?.Losses ?? 0)}", true);
            builder.AddField("RATING", (profile?.Rating ?? 0).ToString("F0"), false);
            builder.AddField("REGION", profile.Region.ToUpper(), true);
            builder.AddField("REGIONAL", $"#{profile.RegionalRank}", true);
            builder.AddField("WORLD", $"#{profile.OverallRank}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }
    }
}
