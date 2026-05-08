namespace OrderGenerator.API.Services;

public interface IShareService
{
    Share Create(string symbol);
    void Save(Share share);
    Share GetBySymbol(string symbol);
    void Process(Order order);
    IEnumerable<ShareResponseDto> GetShares();
    (Result, Share) CreateShareIfNotExist(OrderRequestDto dto);
    Result ValidateTransactionValueAgainstTotalQuantity(Share share, OrderRequestDto dto);
}

public class ShareService : IShareService
{
    public Share Create(string symbol) =>
        new(symbol);

    public void Save(Share share) =>
        InMemoryDb.Share[share.Symbol!] = share;

    public Share GetBySymbol(string symbol) =>
        InMemoryDb.Share.TryGetValue(symbol, out var share) ? share : null!;

    public void Process(Order order)
    {
        var share = InMemoryDb.Share[order.Symbol];
        share.ProcessOrder(order);
        
        RemoveAssetNoPosition(share);
    }

    public IEnumerable<ShareResponseDto> GetShares() =>
        InMemoryDb.Share.Select(x => new ShareResponseDto
        {
            Symbol = x.Key,
            AveragePrice = x.Value.AveragePrice,
            FinancialExposure = x.Value.FinancialExposure,
            TotalAmount = x.Value.TotalAmount
        });

    public (Result, Share) CreateShareIfNotExist(OrderRequestDto dto)
    {
        var share = GetBySymbol(dto.Symbol!);

        if (share is not null)
            return (Result.Ok(), share);
        
        if (share is null && dto.Side.GetValueOrDefault() == Constants.Side.Sell)
            return (Result.Fail("It is not possible to sell an asset that is not in your portfolio."), null)!;

        share = Create(dto.Symbol!);
        Save(share);
        
        return (Result.Ok(), share);
    }

    public Result ValidateTransactionValueAgainstTotalQuantity(Share share, OrderRequestDto dto) =>
        dto.Side == Constants.Side.Sell && dto.Amount > share.TotalAmount ? 
            Result.Fail("The transaction value cannot exceed the total quantity of the asset.") : 
            Result.Ok();

    private static void RemoveAssetNoPosition(Share share)
    {
        if (share.TotalAmount == 0)
            InMemoryDb.Share.Remove(share.Symbol!, out _);
    }
}