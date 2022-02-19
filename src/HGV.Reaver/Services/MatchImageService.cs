using Azure.Storage.Blobs;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        private List<string> STATUS = new List<string>() { "done", "failed", };

        private readonly string windrunUrl;
        private readonly string connectionString;
        private readonly ConnectOptions puppeteerConfuration;
        private readonly IShotstackService shotstackService;

        public MatchImageService(IOptions<ReaverSettings> settings, IShotstackService shotstackService)
        {
            this.windrunUrl = settings?.Value?.WindrunUrl ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.WindrunUrl));
            this.connectionString = settings?.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));

            var token = settings?.Value?.BrowserlessToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BrowserlessToken));
            this.puppeteerConfuration = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };

            this.shotstackService = shotstackService;
        }

        public async Task<Uri> StorageMatchSummaryImage(long matchId)
        {
            var selector = ".match_summary";
            var options = new ViewPortOptions { Width = 755, Height = 720 };
            var image = await GetImage(matchId, 0, selector, options);

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
            await client.UploadAsync(image);

            return client.Uri;
        }

        public async Task<Uri> StorageMatchPlayersImage(long matchId)
        {
            var selector = ".match_players";
            var options = new ViewPortOptions { Width = 1280, Height = 720 };
            var image = await GetImage(matchId, 0, selector, options);

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
            await client.UploadAsync(image);

            return client.Uri;
        }

        public async Task<Uri> StorageMatcDraftAnimation(long matchId)
        {
            try
            {
                var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                var imagesUrls = await GetScreenshots(matchId, cts.Token);
                var tmpMovieUrl = await RenderMovice(imagesUrls, cts.Token);
                var bloblMovieUrl = await StorageMoive(tmpMovieUrl, cts.Token);
                return bloblMovieUrl;
            }
            catch (OperationCanceledException ex)
            {
                throw new UserFriendlyException("The screenshot collection, movie rendering and/or storaging process took too long and was canceled.", ex);
            }
        }

        private async Task<Uri> StorageMoive(Uri tmpMovieUrl, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var key = Guid.NewGuid();
            var moiveBlob = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.mp4");
            var copyOpteration = await moiveBlob.StartCopyFromUriAsync(tmpMovieUrl);
            await copyOpteration.WaitForCompletionAsync(ct);
            return moiveBlob.Uri;
        }

        private async Task<Uri> RenderMovice(IEnumerable<Uri> images, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var renderOpteration = await this.shotstackService.Render(images);
            var result = await renderOpteration.WaitForCompletion(ct);

            return result?.Url ?? throw new InvalidOperationException("The Render Opteration Did Not Return a Value");
        }

        private async Task<IEnumerable<Uri>> GetScreenshots(long matchId, CancellationToken ct)
        {
            var collection = new List<Uri>();

            var selector = ".draft_replay_body";
            var viewPortOptions = new ViewPortOptions 
            { 
                Width = 1280, 
                Height = 768 
            };
            var screenshotOptions = new ScreenshotOptions() 
            { 
                Type = ScreenshotType.Png,
                BurstMode = true,
                OmitBackground = false,
            };
            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(viewPortOptions);
                await page.GoToAsync($"{windrunUrl}/matches/{matchId}");
                await page.WaitForSelectorAsync(selector);

                for (int i = 0; i < 40; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var key = Guid.NewGuid();
                    var imageBlob = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
                    await imageBlob.DeleteIfExistsAsync();
                    collection.Add(imageBlob.Uri);

                    await using var element = await page.QuerySelectorAsync(selector);
                    await using var stream = await element.ScreenshotStreamAsync(screenshotOptions);
                    await imageBlob.UploadAsync(stream);
                    await page.ClickAsync(".next_step ");
                }

                await page.DisposeAsync();
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }

            return collection;
        }


        private async Task<Stream> GetImage(long matchId, int step, string selector, ViewPortOptions options)
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
                var stream = await element.ScreenshotStreamAsync(new ScreenshotOptions() { Type = ScreenshotType.Png });
                await element.DisposeAsync();
                await page.DisposeAsync();

                return stream;
            }
            finally
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }
    }
}
