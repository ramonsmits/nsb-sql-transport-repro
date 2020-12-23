using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace Service
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime().ConfigureLogging(
                    _ =>
                    {
                        _.ClearProviders();
                        _.AddConsole();
                        _.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddHostedService<MyService>();
                    })
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
