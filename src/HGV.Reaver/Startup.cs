using HGV.Basilius.Client;
using HGV.Reaver.Data;
using HGV.Reaver.Factories;
using HGV.Reaver.Hosts;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HGV.Reaver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var settings = Configuration.Get<ReaverSettings>();
            services.Configure<ReaverSettings>(Configuration);
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Discord";
            })
            .AddCookie()
            .AddDiscord(options =>
            {
                options.ClientId = settings?.DiscordClientId ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.DiscordClientId));
                options.ClientSecret = settings?.DiscordClientSecret ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.DiscordClientSecret));
                options.Scope.Add("identify");
                options.Scope.Add("email");
                options.Validate();
            })
            .AddSteam(options =>
            {
                options.ApplicationKey = settings.SteamKey ?? throw new ConfigurationValueMissingException(nameof(ReaverSettings.SteamKey));
                options.Validate();
            });

            services.AddControllersWithViews();

            services.AddHostedService<DiscordLifetimeHost>();

            services.AddHttpClient<IProfileService, ProfileService>();
            services.AddHttpClient<IMatchServices, MatchServices>();
            services.AddHttpClient<IAbilityStatsService, AbilityStatsService>();

            services.AddSingleton<IDiscordClientFactory, DiscordClientFactory>();
            services.AddSingleton<IMatchImageService, MatchImageService>();
            services.AddSingleton<IAbilityImageService, AbilityImageService>();
            services.AddSingleton<IRanksImageService, RanksImageService>();
            services.AddSingleton<IMetaClient, MetaClient>();
            
            services.AddDbContext<ReaverContext>(ServiceLifetime.Transient);

            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IRoleLinkService, RoleLinkService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseWelcomePage("/");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
