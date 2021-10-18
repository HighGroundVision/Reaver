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
    public interface IDraftImageService
    {
        Task<Stream> CreateGif(long matchId);
    }

    public class DraftImageService : IDraftImageService
    {
        private readonly ConnectOptions puppeteerConfuration;
        private readonly IMetaClient metaClient;

        public DraftImageService(IOptions<ReaverSettings> settings, IMetaClient metaClient)
        {
            var token = settings?.Value?.BrowserlessToken ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.BrowserlessToken));
            this.puppeteerConfuration = new ConnectOptions() { BrowserWSEndpoint = $"wss://chrome.browserless.io?token={token}" };

            this.metaClient = metaClient;
        }

        public async Task<Stream> CreateGif(long matchId)
        {
            var tasks = Enumerable.Range(0, 40).Select(step => GetImage(matchId, step)).ToList();
            var images = await Task.WhenAll(tasks);

            using var collection = new MagickImageCollection();
            foreach (var img in images)
            {
                collection.Add(img);
            }

            var stream = new MemoryStream();
            await collection.WriteAsync(stream, MagickFormat.Gif);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private async Task<MagickImage> GetImage(long matchId, int step)
        {
            var browser = await Puppeteer.ConnectAsync(this.puppeteerConfuration);
            try
            {
                var page = await browser.NewPageAsync();
                page.DefaultNavigationTimeout = 0;

                await page.SetViewportAsync(new ViewPortOptions { Width = 1280, Height = 720 });
                var selector = ".draft_replay_body";

                await page.GoToAsync($"http://ad.datdota.com/matches/{matchId}?step={step}");
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
