namespace WeatherWebApi.Model;

public class WeatherForecast
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double Temperature { get; set; }

    public double TemperatureC => (Temperature - 32) * (5 / 9);

    public double TemperatureF => 32 + Temperature / 0.5556;

    public bool IsCelsius { get; set; }

    public string? Summary { get; set; }
}
