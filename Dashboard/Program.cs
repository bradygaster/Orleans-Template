using Orleans;
using Orleans.Clustering.CosmosDB;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.CosmosDB;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .Configure<SiloOptions>(options =>
        {
            options.SiloName = "Dashboard";
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "WeatherApp";
        })
        .Configure<EndpointOptions>(options =>
        {
            options.AdvertisedIPAddress = IPAddress.Loopback;
            options.SiloPort = 11113;
            options.GatewayPort = 30002;
        })
        .UseCosmosDBMembership((CosmosDBClusteringOptions clusteringOptions) =>
        {
            clusteringOptions.ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Direct;
            clusteringOptions.AccountEndpoint = builder.Configuration.GetValue<string>("AccountEndpoint");
            clusteringOptions.AccountKey = builder.Configuration.GetValue<string>("AccountKey");
            clusteringOptions.DB = builder.Configuration.GetValue<string>("DB");
            clusteringOptions.CanCreateResources = true;
        })
        .AddCosmosDBGrainStorageAsDefault((CosmosDBStorageOptions cosmosOptions) =>
        {
            cosmosOptions.AccountEndpoint = builder.Configuration.GetValue<string>("AccountEndpoint");
            cosmosOptions.AccountKey = builder.Configuration.GetValue<string>("AccountKey");
            cosmosOptions.DB = builder.Configuration.GetValue<string>("DB");
            cosmosOptions.CanCreateResources = true;
        })
        .ConfigureApplicationParts(options => options.AddFromApplicationBaseDirectory())
        .UseDashboard(dashboardOptions => dashboardOptions.HostSelf = false)
        ;
});

builder.Services.AddServicesForSelfHostedDashboard();

var app = builder.Build();
app.UseOrleansDashboard();

await app.RunAsync();
