using Azure.Data.Tables;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;


namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Profile", "Profile Commands")]
    public class ProfileCommand : ApplicationCommandModule
    {
        private readonly IAccountService accountService;
        private readonly IProfileService profileService;

        private readonly string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";

        public ProfileCommand(IAccountService accountService, IProfileService profileService)
        {
            this.accountService = accountService;
            this.profileService = profileService;
        }

        [SlashCommand("Card", "Profile Summary")]
        public async Task Card(InteractionContext ctx,
            [Choice("Public", 0)]
            [Choice("Yourself", 1)]
            [Option("share", "Share with whom?")] long share = 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = (share == 1) });

            var user = await this.accountService.GetLinkedAccount(ctx.Guild.Id, ctx.Member.Id);
            var profile = await this.profileService.GetProfile(user.SteamId);

            var builder = new DiscordEmbedBuilder()
                .WithTitle(profile.Nickname)
                .WithUrl($"http://steamcommunity.com/profiles/{user.SteamId}/")
                .WithThumbnail(profile.Avatar ?? DEFAULT_IMAGE_URL)
                .WithColor(DiscordColor.Purple)
                .WithFooter("stats provided by ad.datdota.com", "https://hyperstone.highgroundvision.com/images/wards/observer.png");

            builder.AddField("ID", profile.AccountId.ToString(), false);
            builder.AddField("WINRATE", (profile.WinLoss?.Winrate ?? 0).ToString("P"), true);
            builder.AddField("WIN/LOSE", $"{(profile?.WinLoss?.Wins ?? 0)} - {(profile?.WinLoss?.Losses ?? 0)}", true);
            builder.AddField("RATING", (profile?.Rating ?? 0).ToString("F0"), false);
            builder.AddField("REGION", profile.Region.ToUpper(), true);
            builder.AddField("REGIONAL RANKING", $"#{profile.RegionalRank}", true);
            builder.AddField("WORLD RANKING", $"#{profile.OverallRank}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }
    }
}
