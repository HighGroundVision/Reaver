using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Account", "Account Commands")]
    public class AccountCommands : ApplicationCommandModule
    {
        private readonly string baseUrl;
        private readonly IAccountService accountService;
        private readonly IProfileService profileService;

        public AccountCommands(IOptions<ReaverSettings> settings, IAccountService accountService, IProfileService profileService)
        {
            this.baseUrl = settings.Value?.BaseURL ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BaseURL));

            this.accountService = accountService;
            this.profileService = profileService;
        }

        [SlashCommand("Link", "Links your Discord and Steam accounts")]
        public async Task Link(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Purple);
            builder.WithTitle("Click Here to Link Accounts");
            builder.WithDescription("A number of the commands require linked accounts. Click the link above to sign into discord and steam to link the accounts.");
            builder.WithUrl($"{this.baseUrl}/account/link/{ctx.Guild.Id}");
            builder.WithThumbnail("https://hyperstone.highgroundvision.com/images/hgv/bot-logo.png");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder.Build()));
        }

        [SlashCommand("Delink", "Everyone deserves the right to be forgotten")]
        public async Task Delink(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            await this.accountService.Remove(ctx.Guild.Id, ctx.Member.Id);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }

        [SlashCommand("Refresh","Overrides your discord nickname to match your steam persona and refreshes your cache profile images.")]
        public async Task Refresh(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var account = await this.accountService.Get(ctx.Member.Guild.Id, ctx.Member.Id);
            var dota = await this.profileService.GetDotaProfile(account.SteamId);
            var steam = await this.profileService.GetSteamProfile(account.SteamId);

            var nickname = steam?.Persona ?? dota.Nickname;
            if (nickname is not null)
            {
                await ctx.Member.ModifyAsync(x => x.Nickname = nickname);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }
    }
}
