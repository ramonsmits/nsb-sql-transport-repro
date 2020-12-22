using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
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
                if (this.endpoint != null)
                {
                    await this.endpoint.Stop();
                }
            }
            catch (Exception ex)
            {
                FailFast("Failed to stop correctly.", ex);
            }

            await base.StopAsync(cancellationToken);
        }

        static int count;

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var line = new string('=', 80);

            try
            {
                const string EndpointName = "endpoint name";
                var endpointConfiguration = new EndpointConfiguration(EndpointName);
                endpointConfiguration.UniquelyIdentifyRunningInstance()
                    .UsingNames(EndpointName, Environment.MachineName);


                endpointConfiguration.SendFailedMessagesTo("error");
                endpointConfiguration.AuditProcessedMessagesTo("audit");

                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                ConventionsBuilder conventions = endpointConfiguration.Conventions();
                conventions
                    .DefiningEventsAs(_ => _.Name.EndsWith("Event"))
                    .DefiningCommandsAs(_ => _.Name.EndsWith("Command"))
                    .DefiningMessagesAs(_ => _.Name.EndsWith("Message"));

                TransportExtensions<SqlServerTransport> transport =
                    endpointConfiguration.UseTransport<SqlServerTransport>();

                var subscriptionSettings = transport.SubscriptionSettings();
                subscriptionSettings.DisableSubscriptionCache();
                subscriptionSettings.SubscriptionTableName("subscriptions", "transportSchema");

                transport.Routing().RouteToEndpoint(
                    Assembly.GetAssembly(typeof(MyCommand)),
                    EndpointName);

                transport
                    .Transactions(TransportTransactionMode.ReceiveOnly)
                    .ConnectionString(
                        "Data Source=.;Database=SqlServerSimple;Integrated Security=True;Max Pool Size=100")
                    .TransactionScopeOptions(TimeSpan.FromSeconds(30), IsolationLevel.ReadCommitted)
                    .DefaultSchema("transportSchema");

                count++;
                Console.WriteLine(line);
                Console.WriteLine(count + " Starting");
                this.endpoint = await Endpoint.Start(endpointConfiguration);
                Console.WriteLine(line);
                Console.WriteLine(count + "Started");

                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine(line);
                    Console.WriteLine(count + "Delay");
                    await Task.Delay(1000, token);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(line);
                Console.WriteLine(count + " Error!!!! " + e);
                throw;
            }
        }

        private static async Task OnCriticalError(ICriticalErrorContext context)
        {
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        private static void FailFast(string message, Exception exception)
        {
            try
            {
                Console.WriteLine(
                    $"Fatal error: {message}. Exception = {exception.Message}; Stacktrace = {exception.StackTrace}");
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}
