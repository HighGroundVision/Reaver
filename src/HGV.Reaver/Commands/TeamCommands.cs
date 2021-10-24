using Azure.Data.Tables;
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
    [SlashCommandGroup("Teams", "Teams Commands", false)]
    public class TeamCommands : ApplicationCommandModule
    {
        public TeamCommands()
        {
        }

        [SlashCommand("Promote", "Create a card to promote your team")]
        public async Task Promote(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            //var user = await this.accountService.GetLinkedAccount(ctx.Guild.Id, ctx.Member.Id);
            //var profile = await this.profileService.GetDotaProfile(user.SteamId);
            //var url = await this.ranksImageService.StorageImage(user.SteamId);

            //var builder = new DiscordEmbedBuilder()
            //    .WithTitle(profile.Nickname)
            //    .WithUrl($"http://steamcommunity.com/profiles/{user.SteamId}/")
            //    .WithThumbnail(profile.Avatar ?? DEFAULT_IMAGE_URL)
            //    .WithImageUrl(url)
            //    .WithColor(DiscordColor.Purple);

            //var delta = DateTime.UtcNow - profile.LastMatch;
            //if (delta.HasValue == false)
            //    builder.WithFooter($"Last played to long ago; Go play more Dota.");
            //else if (delta.Value.Days < 1)
            //    builder.WithFooter($"Last played {delta.Value.Hours} hours ago.");
            //else
            //    builder.WithFooter($"Last played {delta.Value.Days} days ago.");

            //builder.AddField("ID", profile.AccountId.ToString(), false);
            //builder.AddField("TOTAL", ((profile?.WinLoss?.Wins ?? 0) + (profile?.WinLoss?.Losses ?? 0)).ToString(), true);
            //builder.AddField("WIN RATE", (profile.WinLoss?.Winrate ?? 0).ToString("P"), true);
            //builder.AddField("RECORD", $"{(profile?.WinLoss?.Wins ?? 0)} - {(profile?.WinLoss?.Losses ?? 0)}", true);
            //builder.AddField("RATING", (profile?.Rating ?? 0).ToString("F0"), false);
            //builder.AddField("REGION", profile.Region.ToUpper(), true);
            //builder.AddField("REGIONAL", $"#{profile.RegionalRank}", true);
            //builder.AddField("WORLD", $"#{profile.OverallRank}", true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Creating Team Card"));
        }

        [SlashCommand("Add", "Add a member to a team")]
        public async Task Add(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        }

        [SlashCommand("Remove", "Remove a member from a team")]
        public async Task Remove(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Updating Team Card"));
        }

        [SlashCommand("Delete", "Deletes the team card")]
        public async Task Delete(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error Deleting the Team Card"));
        }
    }
}
