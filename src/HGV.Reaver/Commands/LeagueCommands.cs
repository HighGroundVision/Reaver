﻿using Azure.Data.Tables;
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
    [SlashCommandGroup("League", "League Commands", false)]
    public class LeagueCommands : ApplicationCommandModule
    {
        public LeagueCommands()
        {
        }

        [SlashCommand("Match", "Create a card handle leauge matches for the week")]
        public async Task Match(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Creating Match Card"));
        }

    }
}