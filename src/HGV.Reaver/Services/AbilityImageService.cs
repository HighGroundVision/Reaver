﻿using Azure.Storage.Blobs;
using HGV.Basilius;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using ImageMagick;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IAbilityImageService
    {
        Task<Uri?> StorageImage(string url, string name);
    }

    public class AbilityImageService : IAbilityImageService
    {
        private const string CONTAINER_NAME = "temp";

        private readonly string connectionString;
        private readonly ConnectOptions puppeteerConfuration;
        private readonly IMetaClient metaClient;

        public AbilityImageService(IOptions<ReaverSettings> settings, IMetaClient metaClient)
        {
            this.connectionString = settings?.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var token = settings?.Value?.BrowserlessToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BrowserlessToken));
            this.puppeteerConfuration = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };

            this.metaClient = metaClient;
        }

        public async Task<Uri?> StorageImage(string url, string name)
        {
            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

                await page.GoToAsync(url);

                await Task.Delay(TimeSpan.FromSeconds(10));

                var elements = await page.QuerySelectorAllAsync(".ability-background");
                var element = await GetElement(elements, name);
                var stream = await element.ScreenshotStreamAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });

                var key = Guid.NewGuid();
                var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
                await client.UploadAsync(stream);
                return client.Uri;
            }
            catch(Exception)
            {
                return null;
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        private async Task<Stream> GetWikiCard(string id)
        {
            var abilityId = int.Parse(id);

            var collection = this.metaClient.GetAbilities();
            var ability = collection.FirstOrDefault(_ => _.Id == abilityId);
            if (ability is null)
                throw new UserFriendlyException($"Unable to find ability {id}");

            var hero = this.metaClient.GetHero(ability.HeroId);
            if(hero is null)
                throw new UserFriendlyException($"Unable to find hero {ability.HeroId}");

            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });

                var slug = hero.Name.Replace(" ", "_");

                if (slug == "Invoker") slug = "Invoker/Ability_Draft";

                await page.GoToAsync($"https://dota2.fandom.com/wiki/{slug}");

                await Task.Delay(TimeSpan.FromSeconds(5));

                var elements = await page.QuerySelectorAllAsync(".ability-background");
                var element = await GetElement(elements, ability.Name);
                var stream = await element.ScreenshotStreamAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });
                return stream;
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        private static async Task<ElementHandle> GetElement(ElementHandle[] elements, string name)
        {
            foreach (var element in elements)
            {
                var property = await element.GetPropertyAsync("innerText");
                var body = await property.JsonValueAsync<string>();
                var title = body.Split("\n").FirstOrDefault();
                if (title == name)
                    return element;
                else
                    continue;
            }

            throw new UserFriendlyException($"Unable to find ability {name}");
        }
    }
}