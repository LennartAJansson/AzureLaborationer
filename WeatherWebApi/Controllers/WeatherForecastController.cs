namespace WeatherWebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using WeatherWebApi.Model;
using WeatherWebApi.Services;

[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherForecastService service;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherForecastService service)
    {
        _logger = logger;
        this.service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWeatherForecasts()
    {
        return Ok(await service.ReadAll());
    }

    [HttpPost]
    public async Task<IActionResult> AddForecast([FromBody] WeatherForecast forecast)
    {
        return Ok(await service.Create(forecast));
    }
}
