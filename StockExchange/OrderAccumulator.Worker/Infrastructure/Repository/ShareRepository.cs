namespace OrderAccumulator.Worker.Infrastructure.Repository;

public class ShareRepository : IShareRepository
{
    //banco em memória
    private readonly ConcurrentDictionary<string, Share> _shares = new();

    public Share GetOrAdd(string symbol) =>
        _shares.GetOrAdd(symbol, s => new Share(s));
    
    public bool ExistShareExecuted(string symbol) => 
        _shares.Any(x => x.Key == symbol && 
                    x.Value.Orders.Any(order => order.Status == Status.Executed));
}