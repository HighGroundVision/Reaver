using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using HGV.Reaver.Models.MatchData;
using HGV.Reaver.Services;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Match", "Match Commands")]
    public class MatchCommands : ApplicationCommandModule
    {
        private readonly string windrunUrl;
        private readonly IMatchServices matchServices;
        private readonly IMatchImageService draftImageService;
        private readonly IAccountService accountService;
        private readonly IMetaClient metaClient;

        public MatchCommands(IOptions<ReaverSettings> settings, IMatchServices matchServices, IMatchImageService draftImageService, IAccountService accountService, IMetaClient metaClient)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.matchServices = matchServices;
            this.draftImageService = draftImageService;
            this.accountService = accountService;
            this.metaClient = metaClient;
        }

        [SlashCommand("Card", "Match Summary")]
        public async Task Card(InteractionContext ctx, [Option("MatchId", "The Id of the match")] long matchId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var meta = await this.matchServices.GetMeta(matchId);
            if (meta.Status == MatchMetaStatus.OK && meta.State == MatchMetaStatus.Parsed)
            {
                var match = await this.matchServices.GetMatch(matchId);
                if (match is null)
                    throw new NullReferenceException("MatchCommands::Card::Match");

                var url = await this.draftImageService.StorageMatchPlayersImage(matchId);

                var duration = TimeSpan.FromSeconds(match.Duration ?? 0);
                var victory = match.RadiantWin == true ? "Radiant" : "Dire";
                var builder = new DiscordEmbedBuilder()
                    .WithTitle($"{match.MatchId}")
                    .WithUrl($"{windrunUrl}/matches/{match.MatchId}")
                    .WithColor(DiscordColor.Purple);

                if (url is not null)
                    builder.WithImageUrl(url);

                var radiantAvg = (int)Math.Round(match.Radiant.Average(_ => _.Rating ?? 0), 0);
                var direAvg = (int)Math.Round(match.Dire.Average(_ => _.Rating ?? 0), 0);
                var ratingAvg = (int)Math.Round((radiantAvg + direAvg) / 2.0, 0);

                
                builder.AddField("WINNER", $"{victory}", true);
                builder.AddField("REGION", $"{match.Region}", true);
                builder.AddField("DURATION", $"{(int)duration.TotalMinutes} mins", true);

                builder.AddField("AVERAGE RATING", $"{ratingAvg}", true);
                builder.AddField("RADIANT RATING", $"{radiantAvg}", true);
                builder.AddField("DIRE RATING", $"{direAvg}", true);

                var user = await this.accountService.Get(ctx.Guild.Id, ctx.Member.Id);
                if(user is not null)
                {
                    var accountID = user.GetSteamId().AccountID;
                    var player = match.Radiant.FirstOrDefault(_ => _.SteamId == accountID) ?? match.Dire.FirstOrDefault(_ => _.SteamId == accountID);
                    var hero = metaClient.GetHero(player?.Hero ?? 0);
                    builder.AddField("HERO", $"{hero?.Name ?? "Unknown"}", true);
                    builder.AddField("K/D/A", $"{player?.Kills ?? 0}/{player?.Deaths ?? 0}/{player?.Assists ?? 0}", true);
                    builder.AddField("CS/GPM", $"{player?.LastHits ?? 0}/{player?.Gpm ?? 0}", true);
                }


                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }
            else
            {
                var builder = new DiscordWebhookBuilder();
                builder.WithContent($"The match is not ready yet please try again shortly.");
                await ctx.EditResponseAsync(builder);
            }
        }

        [SlashCommand("Draft", "Animation of Draft")]
        public async Task Draft(InteractionContext ctx, [Option("MatchId", "The Id of the match")] long matchId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var meta = await this.matchServices.GetMeta(matchId);
            if (meta.Status == MatchMetaStatus.OK && meta.State == MatchMetaStatus.Parsed)
            {
                {
                    var builder = new DiscordEmbedBuilder();
                    builder.WithTitle($"{matchId}")
                        .WithDescription("With idiots like these in charge of recording it make take a minute. We will update this message and ping you when the video is ready.")
                        .WithUrl($"{windrunUrl}matches/{matchId}")
                        .WithImageUrl("https://i.imgur.com/WhAjDPV.jpg");
                    
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }

                var match = await this.matchServices.GetMatch(matchId);
                var url = await this.draftImageService.StorageMatcDraftAnimation(matchId);

                {
                    var builder = new DiscordWebhookBuilder();
                    builder.WithContent($"{ctx.Member.Mention} the video of the draft for match {match.MatchId} is ready. {Environment.NewLine} {url}");                 
                    await ctx.EditResponseAsync(builder);
                }
            }
            else
            {
                var builder = new DiscordWebhookBuilder();
                builder.WithContent($"The match is not ready yet please try again shortly.");
                await ctx.EditResponseAsync(builder);
            }
        }
    }
}