using Abstractions;
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
            options.SiloName = "Silo";
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "WeatherApp";
        })
        .Configure<EndpointOptions>(options =>
        {
            options.AdvertisedIPAddress = IPAddress.Loopback;
            options.SiloPort = 11111;
            options.GatewayPort = 30000;
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
        });
});

var app = builder.Build();

app.MapGet("/", () => "Orleans Silo is running.");

app.MapGet("/weather/{city}", async (string city, IGrainFactory grainFactory) =>
{

    var cityGrain = grainFactory.GetGrain<IWeatherStatusGrain>(city);
    var result = await cityGrain.GetWeatherStatusAsync();
    return result;
})
.Produces<WeatherStatusInfo>(statusCode: StatusCodes.Status200OK);

await app.RunAsync();
