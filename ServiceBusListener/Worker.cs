namespace ServiceBusListener;

using Azure.Messaging.ServiceBus;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> logger;
    private readonly ServiceBusProcessor processor;

    public Worker(ILogger<Worker> logger, ServiceBusClient client, IConfiguration configuration)
    {
        this.logger = logger;
        processor = client.CreateProcessor(configuration["ServiceBus:Queue"]);
        processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
        processor.ProcessErrorAsync += Processor_ProcessErrorAsync;
    }

    private Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        logger.LogError("Error: {msg}", arg.Exception.ToString());

        return Task.CompletedTask;
    }

    private async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs arg)
    {
        logger.LogInformation("Received: {msg}", arg.Message.Body.ToString());

        await arg.CompleteMessageAsync(arg.Message);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await processor.StartProcessingAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(3000, stoppingToken);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync -= Processor_ProcessMessageAsync;
        processor.ProcessErrorAsync -= Processor_ProcessErrorAsync;

        await processor.CloseAsync();
        //await processor.StopProcessingAsync();
        await base.StopAsync(cancellationToken);
    }
}
