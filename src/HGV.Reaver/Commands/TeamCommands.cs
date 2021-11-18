using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Data;
using HGV.Reaver.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace HGV.Reaver.Commands
{

    [SlashCommandGroup("Teams", "Teams Commands", false)]
    public class TeamCommands : ApplicationCommandModule
    {
        private const string DEFAULT_IMAGE_URL = "https://steamuserimages-a.akamaihd.net/ugc/868480752636433334/1D2881C5C9B3AD28A1D8852903A8F9E1FF45C2C8/";
        private const string DEFAULT_IMAGE_WARD = "https://hyperstone.highgroundvision.com/images/wards/observer.png";
        
        private readonly IAccountService accountService;

        public TeamCommands(IAccountService accountService)
        {
            this.accountService = accountService;
        }

        [SlashCommand("Create", "Answer some questions to create a team")]
        public async Task Create(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not Implemented."));
        }

        [SlashCommand("Import", "Improt a team from an existing sytem")]
        public async Task Import(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not Implemented."));
        }

        [SlashCommand("Promote", "Promote your team")]
        public async Task Promote(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Not Implemented."));
        }

        //[SlashCommand("Add", "Add a member to a team")]
        //public async Task Add(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        //}

        //[SlashCommand("Remove", "Remove a member from a team")]
        //public async Task Remove(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        //}

        //[SlashCommand("Delete", "Deletes the team card")]
        //public async Task Delete(InteractionContext ctx)
        //{
        //    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

        //    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Deleting the Team Card"));
        //}
    }
}
