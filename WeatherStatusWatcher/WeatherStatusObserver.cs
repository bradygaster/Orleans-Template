using Abstractions;
using Microsoft.Extensions.Logging;

namespace WeatherStatusWatcher
{
    public class WeatherStatusObserver : IWeatherStatusObserver
    {
        public ILogger<WeatherStatusObserver> Logger { get; set; }

        public WeatherStatusObserver(ILogger<WeatherStatusObserver> logger)
        {
            Logger = logger;
        }

        public void OnWeatherUpdated(WeatherStatusInfo info)
        {
            Logger.LogInformation($"Saving {info.City}'s weather to {info.Fahrenheit} and {info.Description}");
        }
    }
}
