namespace WeatherWebApi.Data;

using Microsoft.EntityFrameworkCore;
using WeatherWebApi.Model;

public interface IWeatherForecastContext: IDbContext
{
    DbSet<WeatherForecast> WeatherForecasts { get; }

    void EnsureMigrations();
}
