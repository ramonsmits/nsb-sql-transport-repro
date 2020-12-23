using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using NServiceBus;
using Service;

class EndpointFactory
{
    public static EndpointConfiguration GetConfiguration()
    {
        const string EndpointName = "endpoint name";
        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.UniquelyIdentifyRunningInstance()
            .UsingNames(EndpointName, Environment.MachineName);

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        endpointConfiguration.DefineCriticalErrorAction(async context =>
        {
            try
            {
                await context.Stop();
            }
            finally
            {
                var exception = context.Exception;
                try
                {
                    Console.WriteLine($"Fatal error: {exception}");
                }
                finally
                {
                    Environment.FailFast("Critical error", exception);
                }
            }
        });

        var conventions = endpointConfiguration.Conventions();
        conventions
            .DefiningEventsAs(_ => _.Name.EndsWith("Event"))
            .DefiningCommandsAs(_ => _.Name.EndsWith("Command"))
            .DefiningMessagesAs(_ => _.Name.EndsWith("Message"));

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();

        var subscriptionSettings = transport.SubscriptionSettings();
        subscriptionSettings.DisableSubscriptionCache();
        subscriptionSettings.SubscriptionTableName("subscriptions", "transportSchema");

        transport.Routing().RouteToEndpoint(Assembly.GetAssembly(typeof(MyCommand)), EndpointName);

        transport
            .Transactions(TransportTransactionMode.ReceiveOnly)
            .ConnectionString(
                "Data Source=.;Database=SqlServerSimple;Integrated Security=True;Max Pool Size=100")
            .TransactionScopeOptions(TimeSpan.FromSeconds(30), IsolationLevel.ReadCommitted)
            .DefaultSchema("transportSchema");

        return endpointConfiguration;
    }

    public static Task<IEndpointInstance> Create()
    {
        return Endpoint.Start(GetConfiguration());
    }
}
