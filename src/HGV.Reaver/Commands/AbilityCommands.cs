using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Commands
{

    public class AbilityAutocompleteProvider : IAutocompleteProvider
    {
        public AbilityAutocompleteProvider()
        {
        }

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            var client = ctx.Services.GetService<IMetaClient>() ?? throw new NullReferenceException("AbilityAutocompleteProvider::Provider::IMetaClient");

            var collection = new List<DiscordAutoCompleteChoice>();

            var option = ctx.OptionValue?.ToString()?.ToLower() ?? string.Empty;
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

            await Task.Delay(1);

           return choices;
        }
    }

    [SlashCommandGroup("Ability", "Commands for infomation about the ability.")]
    public class AbilityCommands : ApplicationCommandModule
    {
        //private readonly string DEFAULT_FOOTER_URL = "https://hyperstone.highgroundvision.com/images/wards/observer.png";

        private readonly string windrunUrl;
        private readonly IAbilityStatsService abilityStatsService;
        private readonly IAbilityImageService abilityImageService;
        private readonly IMetaClient metaClient;

        public AbilityCommands(IOptions<ReaverSettings> settings, IAbilityStatsService abilityStatsService, IAbilityImageService abilityImageService, IMetaClient metaClient)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.abilityStatsService = abilityStatsService ?? throw new NullReferenceException("AbilityCommands::IAbilityStatsService");
            this.abilityImageService = abilityImageService ?? throw new NullReferenceException("AbilityCommands::IAbilityImageService");
            this.metaClient = metaClient ?? throw new NullReferenceException("AbilityCommands::IMetaClient");
        }

        [SlashCommand("Card", "A card with details from the wiki including stats.")]
        public async Task Card(InteractionContext ctx,
            [Autocomplete(typeof(AbilityAutocompleteProvider)), Option("Ability", "Enter the ability Id or the we will supply a list to select your choice.", true)] string id
        )
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var ability = await this.abilityStatsService.GetAbility(id);

            await CreateMessage(ctx, ability, null);

            var hero = this.metaClient.GetHero(ability.HeroId);
            if (hero is null)
                throw new UserFriendlyException($"Unable to find hero {ability.HeroId}");

            var heroSlug = hero.Name.Replace(" ", "_");
            if (heroSlug == "Invoker")
                heroSlug = "Invoker/Ability_Draft";

            var wikiUrl = $"https://dota2.fandom.com/wiki/{heroSlug}";
            var imageUrl = await this.abilityImageService.StorageImage(wikiUrl, ability.Name ?? string.Empty);

            await CreateMessage(ctx, ability, imageUrl);
        }

        private async Task CreateMessage(InteractionContext ctx, Models.Abilities.AbilityStat ability, Uri? imageUrl)
        {
            var builder = new DiscordEmbedBuilder()
                .WithTitle(ability.Name)
                .WithDescription(ability.Description)
                .WithUrl($"{windrunUrl}/abilities/{ability.AbilityId}/")
                .WithColor(DiscordColor.Purple);

            if (imageUrl is not null)
                builder.WithImageUrl(imageUrl);

            builder.AddField("AVG PICK", $"#{Math.Round(ability.AvgPickPosition, 0)}", true);
            builder.AddField("PICK RATE", (ability.PickRate).ToString("P"), true);
            builder.AddField("WIN RATE", (ability.Winrate).ToString("P"), true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
        }
    }
}