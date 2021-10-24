using Azure.Data.Tables;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Profile", "Profile Commands")]
    public class ProfileCommands : ApplicationCommandModule
    {
        private const string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";

        private readonly IAccountService accountService;
        private readonly IProfileService profileService;
        private readonly IRanksImageService ranksImageService;

        public ProfileCommands(IAccountService accountService, IProfileService profileService, IRanksImageService RanksImageService)
        {
            this.accountService = accountService;
            this.profileService = profileService;
            this.ranksImageService = RanksImageService;
        }

        [SlashCommand("Card", "Profile Summary")]
        public async Task Card(InteractionContext ctx,
            [Choice("Public", 0)]
            [Choice("Yourself", 1)]
            [Option("share", "Share with whom?")] long share = 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = (share == 1) });

            var user = await this.accountService.GetLinkedAccount(ctx.Guild.Id, ctx.Member.Id);
            var profile = await this.profileService.GetDotaProfile(user.SteamId);
            var url = await this.ranksImageService.StorageImage(user.SteamId);

            var builder = new DiscordEmbedBuilder()
                .WithTitle(profile.Nickname)
                .WithUrl($"http://steamcommunity.com/profiles/{user.SteamId}/")
                .WithThumbnail(profile.Avatar ?? DEFAULT_IMAGE_URL)
                .WithImageUrl(url)
                .WithColor(DiscordColor.Purple);

            var delta = DateTime.UtcNow - profile.LastMatch;
            if (delta.HasValue == false)
                builder.WithFooter($"Last played to long ago; Go play more Dota.");
            else if (delta.Value.Days < 1)
                builder.WithFooter($"Last played {delta.Value.Hours} hours ago.");
            else
                builder.WithFooter($"Last played {delta.Value.Days} days ago.");

            builder.AddField("ID", profile.AccountId.ToString(), false);
            builder.AddField("TOTAL", ((profile?.WinLoss?.Wins ?? 0) + (profile?.WinLoss?.Losses ?? 0)).ToString(), true);
            builder.AddField("WIN RATE", (profile.WinLoss?.Winrate ?? 0).ToString("P"), true);
            builder.AddField("RECORD", $"{(profile?.WinLoss?.Wins ?? 0)} - {(profile?.WinLoss?.Losses ?? 0)}", true);
            builder.AddField("RATING", (profile?.Rating ?? 0).ToString("F0"), false);
            builder.AddField("REGION", profile.Region.ToUpper(), true);
            builder.AddField("REGIONAL", $"#{profile.RegionalRank}", true);
            builder.AddField("WORLD", $"#{profile.OverallRank}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }
    }
}
