using HGV.Reaver.Bot.Hosts;
using HGV.Reaver.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HGV.Reaver.Bot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .ConfigureLogging(logger => { })
                .ConfigureServices(ConfigureServices)
                .RunConsoleAsync();
        }

        static private void ConfigureServices(HostBuilderContext _, IServiceCollection services)
        {
            services.AddHttpClient<IProfileService, ProfileService>();
            services.AddHostedService<DiscordLifetimeHost>();       
        }
    }
}
