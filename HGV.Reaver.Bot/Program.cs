using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HGV.Reaver.Bot
{
    class Program
    {
        static Task Main(string[] args)
        {
            using IHost host = Host
                .CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            return host.RunAsync();
        }

        static private void ConfigureServices(HostBuilderContext _, IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddHostedService<DiscordLifetimeHost>();
        }
    }
}
