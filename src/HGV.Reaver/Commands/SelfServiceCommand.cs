using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Data;
using HGV.Reaver.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("SelfService", "Commmands to build new or add to self service to existing messages", false)]
    public class SelfServiceCommand : ApplicationCommandModule
    {
        
        private readonly IRoleLinkService roleLinkService;


        public SelfServiceCommand(IRoleLinkService roleLinkService)
        {
            this.roleLinkService = roleLinkService;
        }


        [SlashCommand("Link", "Links a role to each reaction.")]
        public async Task Link(InteractionContext ctx,
            [Option("MessageLink", "The url of the msg from Copy Message Link context menu.")] string msgLink)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var builder = new DiscordEmbedBuilder()
                .WithTitle("Linked Roles to Reactions")
                .WithUrl(msgLink);

            var trash = new List<DiscordMessage>();

            try
            {
                if (msgLink is null)
                    throw new UserFriendlyException("Message Link Required.");

                var startsWith = msgLink.StartsWith("https://discord.com/channels/");
                if (!startsWith)
                    throw new UserFriendlyException("Invaild Message Link Format.");

                var identities = msgLink.Replace("https://discord.com/channels/", "").Split("/").Select(_ => ulong.Parse(_)).ToList();
                var (guildId, channelId, msgId) = identities;

                var channel = ctx.Guild.GetChannel(channelId);
                if (channel is null)
                    throw new UserFriendlyException("Channel not found.");

                var sourceMessage = await channel.GetMessageAsync(msgId);
                if (sourceMessage is null)
                    throw new UserFriendlyException("Message not found.");

                foreach (var reaction in sourceMessage.Reactions)
                {
                    var msg = await ctx.Channel.SendMessageAsync($"Please repond with a Role for Reaction: {reaction.Emoji}");
                    trash.Add(msg);

                    var response = await ctx.Channel.GetNextMessageAsync();
                    if (response.TimedOut)
                        continue;

                    trash.Add(response.Result);

                    foreach (var role in response.Result.MentionedRoles)
                    {
                        var entity = new RoleLinkEntity()
                        {
                            GuidId = ctx.Guild.Id,
                            MessageId = sourceMessage.Id,
                            EmojiName = reaction.Emoji.GetDiscordName(),
                            RoleId = role.Id,
                        };

                        builder.AddField(entity.EmojiName, role.Name);

                        await this.roleLinkService.Add(entity);
                    }
                }
            }
            finally
            {
                await ctx.Channel.DeleteMessagesAsync(trash);
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }


        [SlashCommand("Clear", "Clears all links to a message.")]
        public async Task Clear(InteractionContext ctx,
            [Option("MessageLink", "The url of the msg from Copy Message Link context menu.")] string msgLink)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            if (msgLink is null)
                throw new UserFriendlyException("Message Link Required.");

            var startsWith = msgLink.StartsWith("https://discord.com/channels/");
            if (!startsWith)
                throw new UserFriendlyException("Invaild Message Link Format.");

            var identities = msgLink.Replace("https://discord.com/channels/", "").Split("/").Select(_ => ulong.Parse(_)).ToList();
            var (guildId, channelId, msgId) = identities;

            await this.roleLinkService.Remove(guildId, msgId);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed all roles links for this message."));
        }

        [SlashCommand("Purge", "Purge all roles links.")]
        public async Task Purge(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await this.roleLinkService.Purge(ctx.Guild.Id);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Removed all linked roles."));
        }

}
}