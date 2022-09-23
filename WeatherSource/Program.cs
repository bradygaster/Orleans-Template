using Orleans;
using Orleans.Clustering.CosmosDB;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.CosmosDB;
using System.Net;
using WeatherSource;

IHost host = Host.CreateDefaultBuilder(args)
        .UseOrleans((context, siloBuilder) =>
        {
            siloBuilder
                .Configure<SiloOptions>(options =>
                {
                    options.SiloName = "WeatherSource";
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "WeatherApp";
                })
                .Configure<EndpointOptions>(options =>
                {
                    options.AdvertisedIPAddress = IPAddress.Loopback;
                    options.SiloPort = 11114;
                    options.GatewayPort = 30003;
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
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
