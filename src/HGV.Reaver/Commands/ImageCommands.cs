using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Reaver.Models;
using HGV.Reaver.Models.Meta;
using HGV.Reaver.Services;
using ImageMagick;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    [SlashCommandGroup("Image", "Image Commands")]
    public class ImageCommands : ApplicationCommandModule
    {
        
        private readonly IMatchServices matchServices;
        private readonly IDraftImageService draftImageService;

        public ImageCommands(IMatchServices matchServices, IDraftImageService draftImageService)
        {
            this.matchServices = matchServices;
            this.draftImageService = draftImageService;
        }

        [SlashCommand("Draft", "Will generate a gif of the steps in the draft")]
        public async Task Draft(InteractionContext ctx, 
            [Option("MatchId","The Id of the match")] long matchId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder());

            var meta = await this.matchServices.GetMeta(matchId);
            if(meta.Status == MatchMetaStatus.OK && meta.State == "parsed")
            {
                var stream = await this.draftImageService.CreateGif(matchId);

                var builder = new DiscordWebhookBuilder();
                builder.AddFile($"match.{matchId}.gif", stream);

                await ctx.EditResponseAsync(builder);
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