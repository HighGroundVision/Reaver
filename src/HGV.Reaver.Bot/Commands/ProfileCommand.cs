using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Bot.Services;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot.Commands
{
    public class ProfileCommand : ApplicationCommandModule
    {
        private readonly IProfileService profileService;

        private readonly string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";

        public ProfileCommand(IProfileService profileService)
        {
            this.profileService = profileService;
        }

        // [ContextMenuRequirePermissions(Permissions.Administrator)]
        // DefaultPermission = false to disable to everyone; then use EditApplicationCommandPermissionsAsync to control the access
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Ability Draft Profile")]
        public async Task GetProfile(ContextMenuContext ctx) 
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            // Database ctx.TargetUser.Id => SteamId

            ulong id = 76561197973295540;
            var profile = await this.profileService.GetProfile(id);

            var builder = new DiscordEmbedBuilder()
                .WithTitle(profile.Nickname)
                .WithUrl($"http://steamcommunity.com/profiles/{id}/")
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
