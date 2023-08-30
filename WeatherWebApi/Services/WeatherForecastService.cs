namespace WeatherWebApi.Services;

using WeatherWebApi.Data;

using WeatherWebApi.Model;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastContext context;

    public WeatherForecastService(IWeatherForecastContext context) => this.context = context;

    public async Task<WeatherForecast?> Create(WeatherForecast forecast)
    {
        if (context.WeatherForecasts.Any(f => f.Id == forecast.Id))
        {
            return null;
        }

        _ = context.Add(forecast);
        _ = await context.SaveChangesAsync();

        return forecast;
    }

    public Task<WeatherForecast?> ReadById(int Id)
    {
        WeatherForecast? forecast = context.WeatherForecasts.FirstOrDefault(x => x.Id == Id);

        return Task.FromResult(forecast);
    }

    public Task<IEnumerable<WeatherForecast>> ReadAll() => Task.FromResult(context.WeatherForecasts.AsEnumerable());

    public async Task<WeatherForecast?> Update(WeatherForecast forecast)
    {
        if (!context.WeatherForecasts.Any())
        {
            return null;
        }

        _ = context.Update(forecast);
        _ = await context.SaveChangesAsync();

        return forecast;
    }

    public async Task<WeatherForecast?> Delete(WeatherForecast forecast)
    {
        if (!context.WeatherForecasts.Any())
        {
            return null;
        }

        _ = context.Remove(forecast);
        _ = await context.SaveChangesAsync();

        return forecast;
    }
}
