using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Basilius.Client;
using HGV.Reaver.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{
    public class AbilityAutocompleteProvider : IAutocompleteProvider
    {
        private readonly IMetaClient client = new MetaClient();

        public AbilityAutocompleteProvider()
        {
        }

        public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var collection = new List<DiscordAutoCompleteChoice>();

            var option = ctx.OptionValue.ToString().ToLower();
            var heroes = client.GetHeroes();
            var abilties = client.GetAbilities();

            var filtered = abilties
                .Where(_ => _.Name is not null)
                .Where(_ => _.Name != string.Empty)
                .Where(_ => _.IsSkill == true || _.IsUltimate == true)
                .Where(_ => _.Name.ToLower().Contains(option))
                .OrderBy(_ => _.Name)
                .ToList();

            var choices = filtered
                .Select(_ => new DiscordAutoCompleteChoice($"{_.Name} ({_.Id})", _.Id.ToString()))
                .Take(10)
                .ToList();

            return Task.FromResult(choices.AsEnumerable());
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
            [Autocomplete(typeof(AbilityAutocompleteProvider)), Option("Ability", "Enter the ability Id or the we will supply a list to select your choice.", true)] string id
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var ability = await this.abilityStatsService.GetAbility(id);

            var builder = new DiscordEmbedBuilder()
                .WithTitle(ability.Name)
                .WithDescription(ability.Description)
                .WithUrl($"http://ad.datdota.com/abilities/{ability.AbilityId}/")
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
            [Autocomplete(typeof(AbilityAutocompleteProvider)), Option("Ability", "Enter the ability Id or the we will supply a list to select your choice.", true)] string id
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