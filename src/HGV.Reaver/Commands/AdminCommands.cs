using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using ImageMagick;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    public class CommandChoiceProvider : ChoiceProvider
    {
        public CommandChoiceProvider()
        {
        }

        public async override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            var collection = new List<DiscordApplicationCommandOptionChoice>();

            var client = this.Services.GetService(typeof(DiscordClient)) as DiscordClient;
            var commands = await client.GetGlobalApplicationCommandsAsync();
            var privateCommands = commands.Where(i => i.DefaultPermission == false);

            foreach (var cmd in privateCommands)
            {
                if(cmd.Type == ApplicationCommandType.SlashCommand)
                {
                    var choice = new DiscordApplicationCommandOptionChoice($"/{cmd.Name}", cmd.Id.ToString());
                    collection.Add(choice);
                }
                else if (cmd.Type == ApplicationCommandType.UserContextMenu)
                {
                    var choice = new DiscordApplicationCommandOptionChoice($"ctx/{cmd.Name}", cmd.Id.ToString());
                    collection.Add(choice);
                }
                else if (cmd.Type == ApplicationCommandType.MessageContextMenu)
                {
                    var choice = new DiscordApplicationCommandOptionChoice($"msg/{cmd.Name}", cmd.Id.ToString());
                    collection.Add(choice);
                }
            }

            return collection;
        }
    }

    [SlashRequireOwner]
    [SlashCommandGroup("Admin", "Admin commands to setup the bot")]
    public class AdminCommands : ApplicationCommandModule
    {
        public AdminCommands()
        {
        }

        [SlashCommand("Permissions", "Links a disabled command with a role to enabled it.")]
        public async Task Permissions(InteractionContext ctx,
            [ChoiceProvider(typeof(CommandChoiceProvider)), Option("command", "Command to link")] string id,
            [Option("role", "Role to link")] DiscordRole role)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var command = await ctx.Client.GetGlobalApplicationCommandAsync(ulong.Parse(id));
            if (command is null)
                throw new UserFriendlyException("Check your option and try again.");

            var permissions = new List<DiscordApplicationCommandPermission>();
            permissions.Add(new DiscordApplicationCommandPermission(role, true));
            await ctx.Guild.EditApplicationCommandPermissionsAsync(command, permissions);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
        }
    }
}