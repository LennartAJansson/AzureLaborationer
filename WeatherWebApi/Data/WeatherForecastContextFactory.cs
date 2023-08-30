namespace WeatherWebApi.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

/*
Add UserSecrets:
  "ConnectionStrings": {
    "WeatherForecastDb": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=weatherforecastdb;Integrated Security=True"
  }

For each migration run:
Add-Migration MigrationName -Context WeatherForecastContext -Project WeatherWebApi -StartupProject WeatherWebApi 
Update-Database -Context WeatherForecastContext -Project WeatherWebApi -StartupProject WeatherWebApi
*/

public class WeatherForecastContextFactory
    : IDesignTimeDbContextFactory<WeatherForecastContext>
{
    public WeatherForecastContext CreateDbContext(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<WeatherForecastContext>()
            .Build();

        DbContextOptionsBuilder<WeatherForecastContext> optionsBuilder = new();
        _ = optionsBuilder.UseSqlServer(configuration.GetConnectionString("WeatherForecastDb")
            ?? throw new ArgumentException("No connectionstring in UserSecret"))
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        return new WeatherForecastContext(optionsBuilder.Options);
    }
}
