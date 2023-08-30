namespace UsingKeyVault;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System.Threading.Tasks;

public class Executor
{
    //Injectad ILogger som möjliggör att vi använder logger.LogInformation
    //i stället för Console.WriteLine
    private readonly ILogger<Executor> logger;

    //Injectad instans av vår Dataklass
    private readonly AppSecret appSecret;

    //Här i constructor sker dependency injection
    //När vi ber ServiceProvider att skapa en instans genom GetRequiredService<Executor>
    //Så undersöker ServiceProvider om den kan skapa denna baserat på vad som behövs
    //i constructorn. Den skapar allt som behövs.om den kan det...
    public Executor(ILogger<Executor> logger, IOptions<AppSecret> options)
    {
        this.logger = logger;
        //Att vår AppSecret injectas inbäddad i en IOptions är tack vare att vi angav det 
        //med Configure<AppSecret> i Program.cs.
        //Denna inbäddning är för att man ska kunna övervaka dynamiskt på uppdateringar mm
        //Själva AppSecret'en finns inbäddad som dess Value-property
        appSecret = options.Value;
    }

    public Task Execute()
    {
        //Detta använder vi i stället för Console.WriteLine
        //{secret} kommer att ersättas av appSecret.MyAppSecret automatiskt
        logger.LogInformation("The appsecret was: {secret}", appSecret.MyAppSecret);

        return Task.CompletedTask;
    }
}