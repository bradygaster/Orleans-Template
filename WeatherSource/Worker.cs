using Abstractions;
using Orleans;

namespace WeatherSource
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IGrainFactory _grainFactory;

        static string[] conditions => "Sunny,Cloudy,Rainy,Party Cloudy,Thunderstorms,Windy,Overcast".Split(',');
        public List<string> Cities { get; set; } = new();

        public Worker(ILogger<Worker> logger, IGrainFactory grainFactory)
        {
            _logger = logger;
            _grainFactory = grainFactory;

            CountryData.CountryLoader.LoadUnitedStatesLocationData()
                .States
                    .First(x => x.Code == "WA")
                        .Provinces.ToList()
                            .ForEach(province => 
                                Cities.Add($"{province.Name}"));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Parallel.ForEachAsync<string>(Cities, stoppingToken, async (city, token) =>
                {
                    var randomWeatherStatus = new WeatherStatusInfo
                    {
                        City = city,
                        Description = conditions[Random.Shared.Next(0, conditions.Length)],
                        Fahrenheit = Random.Shared.Next(60, 90)
                    };

                    _logger.LogInformation($"Sending {randomWeatherStatus.City} weather at {DateTime.Now}: " +
                        $"{randomWeatherStatus.Fahrenheit} degrees and {randomWeatherStatus.Description}");

                    await _grainFactory
                        .GetGrain<IWeatherStatusGrain>(city)
                            .SetWeatherStatusAsync(randomWeatherStatus);

                    _logger.LogInformation($"Sent {randomWeatherStatus.City} weather.");
                });

            }
        }
    }
}