using Abstractions;
using Orleans;

namespace WeatherStatusWatcher
{
    public class Worker : IHostedService
    {
        internal static string City = string.Empty;

        private readonly ILogger<Worker> Logger;
        private readonly IWeatherStatusObserver WeatherStatusObserver;
        private readonly IGrainFactory GrainFactory;
        private IWeatherStatusGrain WeatherStatusGrain; 

        public Worker(ILogger<Worker> logger,
            IWeatherStatusObserver weatherStatusObserver, 
            IGrainFactory grainFactory)
        {
            Logger = logger;
            WeatherStatusObserver = weatherStatusObserver;
            GrainFactory = grainFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            WeatherStatusGrain = GrainFactory.GetGrain<IWeatherStatusGrain>(Worker.City);
            var reference = await GrainFactory.CreateObjectReference<IWeatherStatusObserver>(WeatherStatusObserver);
            await WeatherStatusGrain.Subscribe(reference);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await WeatherStatusGrain.Unsubscribe(WeatherStatusObserver);
        }
    }
}