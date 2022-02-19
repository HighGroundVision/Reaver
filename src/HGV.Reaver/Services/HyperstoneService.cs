using Azure.Storage.Blobs;
using HGV.Basilius;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HGV.Reaver.Services
{
    public interface IHyperstoneService
    {
        Task<Stream> GetImageStream(string url);
        Task<Uri> StorageRosterImage(List<Hero> roster);
    }

    public class HyperstoneService : IHyperstoneService
    {
        private const string CONTAINER_NAME = "temp";
        private readonly string connectionString;
        private readonly HttpClient httpClient;

        public HyperstoneService(HttpClient client, IOptions<ReaverSettings> settings)
        {
            this.connectionString = settings?.Value?.StorageConnectionString ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.StorageConnectionString));
            this.httpClient = client ?? throw new ArgumentNullException(nameof(client));  
        }

        public async Task<Stream> GetImageStream(string url)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await this.httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                return stream;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Uri> StorageRosterImage(List<Hero> roster)
        {
            var images = new List<Image>();
            for (int x = 0; x < roster.Count(); x++)
            {
                var hero = roster.ElementAt(x);
                var stream = await this.GetImageStream(hero.ImageIcon); // 32 x 32
                var data = await Image.LoadWithFormatAsync(stream); 
                images.Add(data.Image);
            }

            var image = new Image<Rgba32>(384, 32, Color.Transparent);
            image.Mutate(ctx =>
            {
                for (int x = 0; x < roster.Count(); x++)
                {
                    var p = new Point(x * 32, 0);
                    ctx.DrawImage(images[x], p, 1f);
                }
            });

            var ms = new MemoryStream();
            await image.SaveAsPngAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var key = Guid.NewGuid();
            var client = new BlobClient(this.connectionString, CONTAINER_NAME, $"{key}.png");
            await client.UploadAsync(ms);
            return client.Uri;
        }
    }
}
