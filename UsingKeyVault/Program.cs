using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Reflection;

using UsingKeyVault;

IHost host = Host.CreateDefaultBuilder(args)

    //Utan Dependency Injection anv�nds en KeyVault client:
    //https://stackoverflow.com/questions/70855868/get-key-vault-serviceclient-within-program-cs-startup-in-a-net-6-app
    //Med Dependency Injection kan vi anv�nda en av de tre metoder som finns h�r:
    //https://medium.com/dotnet-hub/use-azure-key-vault-with-net-or-asp-net-core-applications-read-azure-key-vault-secret-in-dotnet-fca293e9fbb3

    .ConfigureAppConfiguration((context, config) =>
    {
        //L�gg till st�d f�r lokala UserSecrets
        _ = config.AddUserSecrets(Assembly.GetExecutingAssembly());

        //Bygg en tillf�llig IConfigurationRoot som vi kan anv�nda h�r
        IConfigurationRoot settings = config.Build();
        string keyVaultEndpoint = settings["AzureKeyVaultEndpoint"];

        //Skapa en DefaultAzureCredential, den kommer att leta efter en token
        //i v�r anv�ndarprofil f�r att autentisera oss mot Azure
        //s� d�rf�r m�ste vi ha gjort en AZ LOGIN f�re.
        DefaultAzureCredential defaultCredentials = new();

        //L�gg till st�d f�r den keyvault vi skapat
        _ = config.AddAzureKeyVault(new Uri(keyVaultEndpoint), defaultCredentials,
            new AzureKeyVaultConfigurationOptions
            {
                ReloadInterval = TimeSpan.FromMinutes(5)
            });
    })

    .ConfigureServices((context, services) =>
    {
        //Koppla v�r dataklass AppSecret till det v�rde vi har i KeyVault
        //AppSecret--MyAppSecret inneb�r: Sektionen AppSecret och Property MyAppSecret
        //Detta �r exakt s� som v�r dataklass ser ut och d�rf�r kan man mappa dessa till varandra
        _ = services.Configure<AppSecret>(options => context.Configuration.GetSection("AppSecret").Bind(options));

        //L�gg till en exekverande klass till dependency injection
        _ = services.AddScoped<Executor>();
    })
    .Build();

//Starta v�r Host, detta drar ig�ng allt som logging, dependency injection med mera
await host.StartAsync();

//Skapa ett scope som vi kan exekvera v�r Executor-klass i
using IServiceScope scope = host.Services.CreateScope();

//L�t dependency injection skapa Executor med sina beroenden
//Titta i Executor vad som injectas i dess constructor
Executor executor = scope.ServiceProvider.GetRequiredService<Executor>();

await executor.Execute();

//St�ng ner v�r Host och st�da undan alla disposable's
await host.StopAsync();
