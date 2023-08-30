namespace WeatherWebApi;

using Microsoft.EntityFrameworkCore;

using WeatherWebApi.Data;
using WeatherWebApi.Services;

public static class AppExtensions
{
    public static IServiceCollection AddMyDbSupport(this IServiceCollection services, string connectionString)
    {
        _ = services.AddDbContext<IWeatherForecastContext, WeatherForecastContext>(optionsBuilder => optionsBuilder.UseSqlServer(connectionString));
        _ = services.AddTransient<IWeatherForecastService, WeatherForecastService>();

        return services;
    }

    public static WebApplication MakeSureMyDbExists(this WebApplication application)
    {
        using IServiceScope scope = application.Services.CreateScope();
        IWeatherForecastContext context = scope.ServiceProvider.GetRequiredService<IWeatherForecastContext>();
        context.EnsureMigrations();

        return application;
    }
}
