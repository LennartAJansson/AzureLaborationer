namespace WeatherWebApi.Data;

using Microsoft.EntityFrameworkCore;
using WeatherWebApi.Model;

public class WeatherForecastContext : DbContext, IWeatherForecastContext
{
    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

    public WeatherForecastContext(DbContextOptions options)
        : base(options) { }

    public void EnsureMigrations()
    {
        if (Database.GetPendingMigrations().Any())
        {
            Database.Migrate();
        }
    }
}
