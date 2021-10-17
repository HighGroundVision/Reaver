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
    public class AccountCommand : ApplicationCommandModule
    {
        private readonly string baseUrl;
        private readonly IAccountService accountService;

        public AccountCommand(IAccountService accountService, IOptions<ReaverSettings> settings)
        {
            this.accountService = accountService;
            this.baseUrl = settings.Value?.BaseURL ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BaseURL));
        }

        [SlashCommand("Link", "Links your Discord and Steam accounts")]
        public async Task Link(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Purple);
            builder.WithTitle("Link Accounts");
            builder.WithDescription("A number of the commands require that your accounts are linked.");
            builder.WithUrl($"{this.baseUrl}/account/link/{ctx.Guild.Id}");
            builder.WithThumbnail("https://hyperstone.highgroundvision.com/images/hgv/bot-logo.png");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder.Build()));
        }

        [SlashCommand("Delink", "Tells the bot to forget you", false)]
        public async Task Delink(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            await this.accountService.RemoveLink(ctx.Guild.Id, ctx.Member.Id);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }
    }
}
