namespace OrderGenerator.API.Infrastructure.Database;

public static class InMemoryDb
{
    public static ConcurrentDictionary<string, Share> Share { get; } = new();
    public static ConcurrentDictionary<Guid, Order> Order { get; } = new();
}