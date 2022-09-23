using Orleans;

namespace Abstractions
{
    public interface IWeatherStatusGrain : IGrainWithStringKey
    {
        Task<WeatherStatusInfo> GetWeatherStatusAsync();
        Task SetWeatherStatusAsync(WeatherStatusInfo weatherStatusInfo);
        Task Subscribe(IWeatherStatusObserver observer);
        Task Unsubscribe(IWeatherStatusObserver observer);
    }
}
