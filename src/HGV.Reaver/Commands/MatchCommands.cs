using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models.Meta;
using HGV.Reaver.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Match", "Match Commands")]
    public class MatchCommands : ApplicationCommandModule
    {
        private readonly IMatchServices matchServices;
        private readonly IMatchImageService draftImageService;

        public MatchCommands(IMatchServices matchServices, IMatchImageService draftImageService)
        {
            this.matchServices = matchServices;
            this.draftImageService = draftImageService;
        }

        [SlashCommand("Card", "Match Summary")]
        public async Task Card(InteractionContext ctx, [Option("MatchId","The Id of the match")] long matchId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var meta = await this.matchServices.GetMeta(matchId);
            if(meta.Status == MatchMetaStatus.OK && meta.State == MatchMetaStatus.Parsed)
            {
                var match = await this.matchServices.GetMatch(matchId);
                var url = await this.draftImageService.StorageMatchPlayersImage(matchId);

                var duration = TimeSpan.FromSeconds(match.Duration);
                var victory = match.RadiantWin == true ? "Radiant" : "Dire";
                var builder = new DiscordEmbedBuilder()
                    .WithTitle($"{match.MatchId}")
                    .WithUrl($"https://abilitydraft.datdota.com/matches/6279888598")
                    .WithColor(DiscordColor.Purple);

                if (url is not null)
                    builder.WithImageUrl(url);

                var radiantAvg = (int)Math.Round(match.Radiant.Average(_ => _.Rating), 0);
                var direAvg = (int)Math.Round(match.Dire.Average(_ => _.Rating), 0);
                var ratingAvg = (int)Math.Round((radiantAvg + direAvg) / 2.0, 0);

                builder.AddField("WINNER", $"{victory}", true);
                builder.AddField("REGION", $"{match.Region}", true);
                builder.AddField("DURATION", $"{(int)duration.TotalMinutes} mins", true);
                builder.AddField("AVG. RATING", $"{ratingAvg}", true);
                builder.AddField("RADIANT RATING", $"{radiantAvg}", true);
                builder.AddField("DIRE RATING", $"{direAvg}", true);

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
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