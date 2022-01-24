using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using HGV.Reaver.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HGV.Reaver.Services
{
    public interface IShotstackService
    {
        Task<RenderOpteration> Render(IEnumerable<Uri> slides);
    }

    public class ShotstackService : IShotstackService
    {
        private readonly HttpClient client;

        public ShotstackService(IOptions<ReaverSettings> settings, HttpClient client)
        {
            this.client = client;
        }

        public async Task<RenderOpteration> Render(IEnumerable<Uri> sources)
        {
            var clips = new List<Clip>();
            var start = 0.0;

            foreach (var item in sources)
            {
                clips.Add(new Clip()
                {
                    Asset = new ImageAsset(item),
                    Start = start,
                    Length = 0.5,
                });

                start += 0.5;
            }

            var edit = new Edit();
            edit.Disk = "local";
            edit.Output = new Output()
            {
                Format = "mp4", // "gif",
                //Size = new Size() { Width = 640, Height = 384 },
                Size = new Size() { Width = 1280, Height = 768 },
                Fps = 15,
                Repeat = true,
                Quality = "medium",
            };
            edit.Timeline = new Timeline()
            {
                Tracks = new List<Track>()
                {
                    new Track()
                    {
                        Clips = clips
                    }
                }
            };

            var input = JsonConvert.SerializeObject(edit, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            var data = new StringContent(input, Encoding.UTF8, "application/json");
       
            var reponse = await this.client.PostAsync("render", data);
            reponse.EnsureSuccessStatusCode();
            var output = await reponse.Content.ReadAsStringAsync();

            var queuedResponse = JsonConvert.DeserializeObject<Queued>(output) ?? throw new InvalidOperationException("Can Not Deserialize Object 'Queued'");

            return new RenderOpteration(client, queuedResponse.Response.Id);
        }
    }

    public class RenderOpteration
    {
        private readonly HttpClient client;

        public RenderOpteration(HttpClient client, Guid id)
        {
            this.client = client;
            this.Id = id;
            this.HasCompleted = false;
            this.Value = null;
        }

        public Guid Id { get; private set; }

        public  RenderResponse? Value { get; private set; }

        public  bool HasCompleted { get; private set; }

        public async ValueTask<RenderResponse?> UpdateStatus(CancellationToken cancellationToken = default)
        {
            var response = await this.client.GetAsync($"render/{this.Id}?data=false", cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            var render = JsonConvert.DeserializeObject<Render>(json) ?? throw new InvalidOperationException("Can Not Deserialize Object 'Render'");

            this.Value = render.Response;
            this.HasCompleted = (this.Value?.Status == "done" || this.Value?.Status == "failed");

            return this.Value;
        }

        public ValueTask<RenderResponse?> WaitForCompletion(CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.WaitForCompletion(TimeSpan.FromSeconds(30), cancellationToken);
        }

        public async ValueTask<RenderResponse?> WaitForCompletion(TimeSpan pollingInterval, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.UpdateStatus(cancellationToken);

            while (this.HasCompleted == false)
            {
                await Task.Delay(pollingInterval, cancellationToken);
                await this.UpdateStatus(cancellationToken);
            }

            return this.Value;
        }
    }
}
