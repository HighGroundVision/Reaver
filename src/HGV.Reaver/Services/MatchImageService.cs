using Azure.Storage.Blobs;
using HGV.Basilius.Client;
using HGV.Reaver.Models;
using ImageMagick;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IMatchImageService
    {
        Task<Uri> StorageMatchSummaryImage(long matchId);
        Task<Uri> StorageMatchPlayersImage(long matchId);
        Task<Uri> StorageMatcDraftAnimation(long matchId);
    }

    public class MatchImageService : IMatchImageService
    {
        private const string CONTAINER_NAME = "temp";

        private readonly string windrunUrl;
        private readonly string connectionString;
        private readonly ConnectOptions puppeteerConfuration;

        public MatchImageService(IOptions<ReaverSettings> settings)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.connectionString = settings?.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var token = settings?.Value?.BrowserlessToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BrowserlessToken));
            this.puppeteerConfuration = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };
        }

        public async Task<Uri> StorageMatchSummaryImage(long matchId)
        {
            var selector = ".match_summary";
            var options = new ViewPortOptions { Width = 755, Height = 720 };
            var image = await GetImage(matchId, 0, selector, options);

            using var stream = new MemoryStream();
            await image.WriteAsync(stream, MagickFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            image.Dispose();

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
            await client.UploadAsync(stream);

            await stream.DisposeAsync();

            return client.Uri;
        }

        public async Task<Uri> StorageMatchPlayersImage(long matchId)
        {
            var selector = ".match_players";
            var options = new ViewPortOptions { Width = 1280, Height = 720 };
            var image = await GetImage(matchId, 0, selector, options);

            using var stream = new MemoryStream();
            await image.WriteAsync(stream, MagickFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            image.Dispose();

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
            await client.UploadAsync(stream);
            
            await stream.DisposeAsync();

            return client.Uri;
        }

        public async Task<Uri> StorageMatcDraftAnimation(long matchId)
        {
            var selector = ".draft_replay_body";
            var options = new ViewPortOptions { Width = 1280, Height = 720 };

            var collection = new MagickImageCollection();

            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(options);
                await page.GoToAsync($"{windrunUrl}/matches/{matchId}");
                await page.WaitForSelectorAsync(selector);

                for (int i = 0; i < 40; i++)
                {
                    var element = await page.QuerySelectorAsync(selector);
                    var data = await element.ScreenshotDataAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });
                    await element.DisposeAsync();

                    var image = new MagickImage(data) { AnimationDelay = 25 };
                    image.Resize(new Percentage(50));
                    collection.Add(image);

                    await page.ClickAsync(".next_step ");
                }

                await page.DisposeAsync();
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }

            using var stream = new MemoryStream();
            await collection.WriteAsync(stream, MagickFormat.Gif);
            stream.Seek(0, SeekOrigin.Begin);

            collection.Dispose();

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.gif");
            await client.UploadAsync(stream);
            await stream.DisposeAsync();

            return client.Uri;
        }

        private async Task<MagickImage> GetImage(long matchId, int step, string selector, ViewPortOptions options)
        {
            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(options);
                await page.GoToAsync($"{windrunUrl}/matches/{matchId}?step={step}");
                await page.WaitForSelectorAsync(selector);

                var element = await page.QuerySelectorAsync(selector);
                var data = await element.ScreenshotDataAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });
                await element.DisposeAsync();
                await page.DisposeAsync();

                var image = new MagickImage(data) { AnimationDelay = 25 };
                image.Resize(new Percentage(50));
                return image;
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }
    }
}
