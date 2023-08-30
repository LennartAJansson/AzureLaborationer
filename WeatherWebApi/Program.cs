using Azure.Identity;

using Microsoft.EntityFrameworkCore;

using WeatherWebApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string keyVaultUrl = builder.Configuration["AzureKeyVault:Uri"];
if (!builder.Environment.IsDevelopment())
{
    _ = builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());
}

builder.Services.AddMyDbSupport(builder.Configuration.GetConnectionString("WeatherForecastDb")
    ?? throw new ArgumentException("No connectionstring"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MakeSureMyDbExists();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
