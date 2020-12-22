using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NServiceBus;

//using NServiceBus.Transport.SQLServer;

namespace Service
{
    public class MyService : BackgroundService
    {
        private IEndpointInstance endpoint;

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                //if (this.endpoint != null)
                {
                    await this.endpoint.Stop();
                }
            }
            catch (Exception ex)
            {
                Environment.FailFast("Failed to stop correctly.", ex);
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                await EndpointFactory.CreateEndpoint();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
            }
        }
    }
}