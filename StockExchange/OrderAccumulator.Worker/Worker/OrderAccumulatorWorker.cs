namespace OrderAccumulator.Worker.Worker;

public class OrderAccumulatorWorker(IAcceptor acceptor, ILogger<OrderAccumulatorWorker> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting FIX Acceptor...");
        acceptor.Start();

        return Task.CompletedTask;
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping FIX Acceptor...");
        acceptor.Stop();
        await base.StopAsync(cancellationToken);
    }
}