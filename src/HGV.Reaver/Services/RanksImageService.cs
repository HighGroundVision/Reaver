using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IRanksImageService
    {
        Task<Uri> StorageImage(string streamId);
    }

    public class RanksImageService : IRanksImageService
    {
        private const string CONTAINER_NAME = "temp";

        private readonly string connectionString;
        private readonly ConnectOptions puppeteerConfuration;

        public RanksImageService(IOptions<ReaverSettings> settings)
        {
            this.connectionString = settings?.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var client = new BlobContainerClient(this.connectionString, CONTAINER_NAME);
            client.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var token = settings?.Value?.BrowserlessToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BrowserlessToken));
            this.puppeteerConfuration = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };
        }

        public async Task<Uri> StorageImage(string streamId)
        {
            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(new ViewPortOptions { Width = 1280, Height = 720 });

                await page.GoToAsync($"http://ad.datdota.com/players");
                await page.WaitForSelectorAsync("#search-box");
                await page.FocusAsync("#search-box");
                await page.Keyboard.TypeAsync(streamId);

                await page.WaitForSelectorAsync("#ratings-graph");
                var element = await page.QuerySelectorAsync("#ratings-graph");
                var stream = await element.ScreenshotStreamAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });
                await element.DisposeAsync();
                await page.DisposeAsync();

                var id = Guid.NewGuid();
                var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{id}.png");
                await client.UploadAsync(stream);

                return client.Uri;
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }
    }
}
