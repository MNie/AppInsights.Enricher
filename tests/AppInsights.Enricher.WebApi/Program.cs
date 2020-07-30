using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AppInsights.Enricher.WebApi
{
    public class Program
    {
        public static Task Main(string[] args) => CreateWebHostBuilder(args).RunAsync();

        public static IWebHost CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}