using Abstractions;
using Orleans;
using Orleans.Clustering.CosmosDB;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.CosmosDB;
using System.Net;
using WeatherStatusWatcher;

int siloPort = 11112;
int gatewayPort = 30001;

if(args.Length != 3)
{
    Console.WriteLine("Please provide a string parameter representing the city name, a silo port, and a dashboard port. If the city name has a space, please surround the name with double-quotes.");
    Environment.Exit(0);
}
else
{
    Worker.City = args[0];
    siloPort = int.Parse(args[1]);
    gatewayPort = int.Parse(args[2]);
}

IHost host = Host.CreateDefaultBuilder(args)
    .UseOrleans((context, siloBuilder) =>
    {
        siloBuilder
            .Configure<SiloOptions>(options =>
            {
                options.SiloName = "WeatherWatcher";
            })
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "WeatherApp";
            })
            .Configure<EndpointOptions>(options =>
            {
                options.AdvertisedIPAddress = IPAddress.Loopback;
                options.SiloPort = siloPort;
                options.GatewayPort = gatewayPort;
            })
            .UseCosmosDBMembership((CosmosDBClusteringOptions clusteringOptions) =>
            {
                clusteringOptions.ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Direct;
                clusteringOptions.AccountEndpoint = context.Configuration.GetValue<string>("AccountEndpoint");
                clusteringOptions.AccountKey = context.Configuration.GetValue<string>("AccountKey");
                clusteringOptions.DB = context.Configuration.GetValue<string>("DB");
                clusteringOptions.CanCreateResources = true;
            })
            .AddCosmosDBGrainStorageAsDefault((CosmosDBStorageOptions cosmosOptions) =>
            {
                cosmosOptions.AccountEndpoint = context.Configuration.GetValue<string>("AccountEndpoint");
                cosmosOptions.AccountKey = context.Configuration.GetValue<string>("AccountKey");
                cosmosOptions.DB = context.Configuration.GetValue<string>("DB");
                cosmosOptions.CanCreateResources = true;
            });
    })
    .ConfigureServices(services =>
    {
        services.AddTransient<IWeatherStatusObserver, WeatherStatusObserver>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
