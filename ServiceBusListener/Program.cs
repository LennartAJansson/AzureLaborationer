using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

using Microsoft.Extensions.Azure;

using ServiceBusListener;

using System.Reflection;
//https://docs.microsoft.com/en-us/dotnet/api/overview/azure/messaging.servicebus-readme

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        _ = config.AddUserSecrets(Assembly.GetExecutingAssembly());
        IConfigurationRoot settings = config.Build();

        string keyVaultEndpoint = settings.GetConnectionString("AzureKeyVault");

        DefaultAzureCredential defaultCredentials = new();

        _ = config.AddAzureKeyVault(new Uri(keyVaultEndpoint), defaultCredentials,
            new AzureKeyVaultConfigurationOptions
            {
                ReloadInterval = TimeSpan.FromMinutes(5)
            });
    })
    .ConfigureServices((context, services) =>
    {
        services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(context.Configuration["ServiceBus:ConnectionString"]);
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
