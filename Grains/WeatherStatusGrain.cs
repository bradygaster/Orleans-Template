using Abstractions;
using Orleans;
using Orleans.Runtime;
using System;

namespace Grains
{
    public class WeatherStatusGrain : Grain, IWeatherStatusGrain
    {
        public WeatherStatusGrain(
            [PersistentState(nameof(WeatherStatusGrain))]
            IPersistentState<WeatherStatusInfo> weatherStatus, 
            ILogger<WeatherStatusGrain> logger)
        {
            WeatherStatus = weatherStatus;
            Logger = logger;
        }

        private IPersistentState<WeatherStatusInfo> WeatherStatus { get; set; }
        private ILogger<WeatherStatusGrain> Logger { get; set; }
        public Task<WeatherStatusInfo> GetWeatherStatusAsync() => Task.FromResult(WeatherStatus.State);
        public HashSet<IWeatherStatusObserver> Observers { get; set; } = new();

        public async Task SetWeatherStatusAsync(WeatherStatusInfo weatherStatusInfo)
        {
            WeatherStatus.State = weatherStatusInfo;
            WeatherStatus.State.City = this.GetPrimaryKeyString();

            List<IWeatherStatusObserver> failed = null!;
            foreach (var observer in Observers)
            {
                try
                {
                    observer.OnWeatherUpdated(WeatherStatus.State);
                }
                catch(Exception ex)
                {
                    failed ??= new();
                    failed.Add(observer);
                }
            }

            if (failed is not null)
            {
                foreach (var observer in failed)
                {
                    Observers.Remove(observer);
                }
            }
        }

        public Task Subscribe(IWeatherStatusObserver observer)
        {
            Observers.Add(observer);
            return Task.CompletedTask;
        }

        public Task Unsubscribe(IWeatherStatusObserver observer)
        {
            Observers.Remove(observer);
            return Task.CompletedTask;
        }
    }
}
