using Orleans;

namespace Abstractions
{
    public interface IWeatherStatusObserver : IGrainObserver
    {
        void OnWeatherUpdated(WeatherStatusInfo info);
    }
}
