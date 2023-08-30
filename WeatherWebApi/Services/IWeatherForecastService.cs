namespace WeatherWebApi.Services;

using WeatherWebApi.Model;

public interface IWeatherForecastService
{
    Task<WeatherForecast?> Create(WeatherForecast forecast);
    Task<IEnumerable<WeatherForecast>> ReadAll();
    Task<WeatherForecast?> ReadById(int Id);
    Task<WeatherForecast?> Update(WeatherForecast forecast);
    Task<WeatherForecast?> Delete(WeatherForecast forecast);
}
