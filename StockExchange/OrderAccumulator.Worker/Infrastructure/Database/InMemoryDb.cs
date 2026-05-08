namespace OrderAccumulator.Worker.Infrastructure.Database;

public static class InMemoryDb
{
    public static ConcurrentDictionary<string, decimal> Exposures { get; } = new();
}