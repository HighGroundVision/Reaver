using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Basilius.Client;
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
    public class AbilityNameChoiceProvider : ChoiceProvider
    {
        public AbilityNameChoiceProvider()
        {
        }

        public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
        {
            var client = this.Services.GetService(typeof(IMetaClient)) as IMetaClient;
            var abilities = client.GetAbilities();
            var heroes = client.GetHeroes();

            var collection = abilities
                .Where(_ => _.IsSkill == true || _.IsUltimate == true)
                .Join(heroes, _ => _.HeroId, _ => _.Id, (lhs, rhs) => new { Name = $"{rhs.Name} {lhs.Name}", Value = lhs.Id })
                .Select(_ => new DiscordApplicationCommandOptionChoice(_.Name, _.Value));

            return Task.FromResult(collection);
        }
    }

    [SlashCommandGroup("Ability", "Commands for infomation about the ability.")]
    public class AbilityCommands : ApplicationCommandModule
    {
        private readonly string DEFAULT_FOOTER_URL = "https://hyperstone.highgroundvision.com/images/wards/observer.png";

        private readonly IAbilityStatsService abilityStatsService;
        private readonly IAbilityImageService abilityImageService;

        public AbilityCommands(IAbilityStatsService abilityStatsService, IAbilityImageService abilityImageService)
        {
            this.abilityStatsService = abilityStatsService;
            this.abilityImageService = abilityImageService;
        }

        [SlashCommand("Summary", "A basic summary of the ability.")]
        public async Task Summary(InteractionContext ctx,
            [ChoiceProvider(typeof(AbilityNameChoiceProvider)), Option("Ability", "select the Ability of one the options")] int id
            //[Option("Ability", "The name of the Ability")] string name
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var ability = await this.abilityStatsService.GetAbility(id);

            var builder = new DiscordEmbedBuilder()
                .WithTitle(ability.Name)
                .WithDescription(ability.Description)
                .WithUrl($"http://ad.datdota.com/abilities{ability.AbilityId}/")
                .WithThumbnail(ability.Image)
                .WithColor(DiscordColor.Purple)
                .WithFooter(ability.Keywords, DEFAULT_FOOTER_URL);

            builder.AddField("AVG PICK", $"#{Math.Round(ability.AvgPickPosition, 0)}", true);
            builder.AddField("PICK RATE", (ability.PickRate).ToString("P"), true);
            builder.AddField("WIN RATE", (ability.Winrate).ToString("P"), true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }

        [SlashCommand("Card", "The Wiki ability card.")]
        public async Task Card(InteractionContext ctx,
            [ChoiceProvider(typeof(AbilityNameChoiceProvider)), Option("Ability", "select the Ability of one the options")] int id
            //[Option("Ability", "The name of the Ability")] string name
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var stream = await this.abilityImageService.GetWikiCard(id);

            var builder = new DiscordWebhookBuilder();
            builder.AddFile($"{id}.png", stream);

            await ctx.EditResponseAsync(builder);
        }
    }
}