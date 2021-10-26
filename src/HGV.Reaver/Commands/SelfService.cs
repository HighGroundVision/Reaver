using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("SelfService", "Commmands to build new or add to self service to existing messages", false)]
    public class SelfService : ApplicationCommandModule
    {
        public SelfService()
        {
        }

        [SlashCommand("Build", "Interactly creates a reaction roles self service embed")]
        public async Task Build(InteractionContext ctx,
            [Option("channel", "The channel to create the message for the self service embed.")] DiscordChannel channel
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            // Embed for Details on after 
            var embed = new DiscordEmbedBuilder();

            var msg = await channel.SendMessageAsync(embed);

           
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("Remove", "Delete all self service links from the database but the messages remain.")]
        public async Task Remove(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error with SelfService"));
        }

        [SlashCommand("Purge", "Delete all self service links from the database but the messages remain.")]
        public async Task Purge(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error with SelfService"));
        }
    }
}