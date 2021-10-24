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
    [SlashCommandGroup("SelfService", "Commmands to build new or add to self service to existing messages", false)]
    public class SelfService : ApplicationCommandModule
    {
        public SelfService()
        {
        }

        [SlashCommand("Build", "Interactivly build a Reaction Roles embed")]
        public async Task Build(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error with AutoRoles"));
        }

        [SlashCommand("Add", "Links a reaction to self service role")]
        public async Task Add(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error with AutoRoles"));
        }
    }
}