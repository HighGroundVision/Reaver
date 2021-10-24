using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace HGV.Reaver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(b => 
                {
                    b.UseStartup<Startup>();
                });
        }
            
    }
}
