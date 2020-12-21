using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Service
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime().ConfigureLogging(
                    _ =>
                    {
                        _.ClearProviders();
                        _.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddHostedService<MyService>();
                    })
                .Build()
                .RunAsync().ConfigureAwait(false);
        }
    }
}