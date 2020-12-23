using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;

public class MyService : BackgroundService
{
    readonly IHostApplicationLifetime applicationLifetime;
    readonly ILogger logger;
    IEndpointInstance endpoint;

    public MyService(IHostApplicationLifetime applicationLifetime, ILogger<MyService> logger)
    {
        this.applicationLifetime = applicationLifetime;
        this.logger = logger;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (endpoint != null)
            {
                await endpoint.Stop();
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex,"StopAsync");
            Environment.FailFast("Failed to stop correctly.", ex);
        }

        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            endpoint =  await EndpointFactory.Create();
            await Task.Delay(Timeout.InfiniteTimeSpan, ct);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "ExecuteAsync");
            applicationLifetime.StopApplication();
        }
    }
}
