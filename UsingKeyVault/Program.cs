using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Reflection;

using UsingKeyVault;

IHost host = Host.CreateDefaultBuilder(args)

    //Utan Dependency Injection används en KeyVault client:
    //https://stackoverflow.com/questions/70855868/get-key-vault-serviceclient-within-program-cs-startup-in-a-net-6-app
    //Med Dependency Injection kan vi använda en av de tre metoder som finns här:
    //https://medium.com/dotnet-hub/use-azure-key-vault-with-net-or-asp-net-core-applications-read-azure-key-vault-secret-in-dotnet-fca293e9fbb3

    .ConfigureAppConfiguration((context, config) =>
    {
        //Lägg till stöd för lokala UserSecrets
        _ = config.AddUserSecrets(Assembly.GetExecutingAssembly());

        //Bygg en tillfällig IConfigurationRoot som vi kan använda här
        IConfigurationRoot settings = config.Build();
        string keyVaultEndpoint = settings["AzureKeyVaultEndpoint"];

        //Skapa en DefaultAzureCredential, den kommer att leta efter en token
        //i vår användarprofil för att autentisera oss mot Azure
        //så därför måste vi ha gjort en AZ LOGIN före.
        DefaultAzureCredential defaultCredentials = new();

        //Lägg till stöd för den keyvault vi skapat
        _ = config.AddAzureKeyVault(new Uri(keyVaultEndpoint), defaultCredentials,
            new AzureKeyVaultConfigurationOptions
            {
                ReloadInterval = TimeSpan.FromMinutes(5)
            });
    })

    .ConfigureServices((context, services) =>
    {
        //Koppla vår dataklass AppSecret till det värde vi har i KeyVault
        //AppSecret--MyAppSecret innebär: Sektionen AppSecret och Property MyAppSecret
        //Detta är exakt så som vår dataklass ser ut och därför kan man mappa dessa till varandra
        _ = services.Configure<AppSecret>(options => context.Configuration.GetSection("AppSecret").Bind(options));

        //Lägg till en exekverande klass till dependency injection
        _ = services.AddScoped<Executor>();
    })
    .Build();

//Starta vår Host, detta drar igång allt som logging, dependency injection med mera
await host.StartAsync();

//Skapa ett scope som vi kan exekvera vår Executor-klass i
using IServiceScope scope = host.Services.CreateScope();

//Låt dependency injection skapa Executor med sina beroenden
//Titta i Executor vad som injectas i dess constructor
Executor executor = scope.ServiceProvider.GetRequiredService<Executor>();

await executor.Execute();

//Stäng ner vår Host och städa undan alla disposable's
await host.StopAsync();
