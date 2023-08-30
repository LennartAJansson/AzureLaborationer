namespace ServiceBusSender;

using Azure.Messaging.ServiceBus;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly ServiceBusSender sender;

    public Worker(ILogger<Worker> logger, ServiceBusClient client, IConfiguration configuration)
    {
        this.logger = logger;
        sender = client.CreateSender(configuration["ServiceBus:Queue"]);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            string msg = $"Sender running at: {DateTimeOffset.Now}";
            logger.LogInformation("Sending: {msg}", msg);
            await sender.SendMessageAsync(new ServiceBusMessage(msg));
            await Task.Delay(3000, stoppingToken);
        }
    }
}
