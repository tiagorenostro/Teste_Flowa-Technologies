namespace OrderGenerator.API.Services;

public interface IShareService
{
    void Save(Share share);
    void ProcessOperation(Order order);
    Result<ShareResponseDto> GetShare(Guid code);
    IEnumerable<ShareResponseDto> GetShares();
    Result<Share> CreateShareIfNotExist(NewOrderRequestDto dto);
    Result ValidateTransactionValueAgainstTotalQuantity(Share share, NewOrderRequestDto dto);
}

public class ShareService : IShareService
{
    public void ProcessOperation(Order order)
    {
        var share = GetBySymbol(order.Symbol);
        share.ProcessOrder(order);
    }

    public Result<ShareResponseDto> GetShare(Guid code) =>
        InMemoryDb.Share.TryGetValue(code, out var share)
            ? Result<ShareResponseDto>.Ok(ConvertToDto(share))
            : Result<ShareResponseDto>.Fail(ErrorType.NotFound, MessageError.ShareNotFound);
    
    public IEnumerable<ShareResponseDto> GetShares() =>
        InMemoryDb.Share.Where(x => !x.Value.IsNoPosition())
            .Select(x => ConvertToDto(x.Value));

    public Result<Share> CreateShareIfNotExist(NewOrderRequestDto dto)
    {
        var share = GetBySymbol(dto.Symbol!);

        if (share is not null)
            return Result<Share>.Ok(share);
        
        if (dto.IsSellOrderRequest())
            return Result<Share>.Fail(ErrorType.Validation, MessageError.NotHavingAssetsForSale);

        var createShareResult = Share.Create(dto.Symbol!);

        if (!createShareResult.Success)
            return createShareResult;

        share = createShareResult.Value;
        
        return Result<Share>.Ok(share);
    }
    
    private static ShareResponseDto ConvertToDto(Share share) => new(share);

    private static Share GetBySymbol(string symbol) =>
        InMemoryDb.Share.FirstOrDefault(x => x.Value.Symbol == symbol).Value;
    
    public void Save(Share share) =>
        InMemoryDb.Share[share.Code] = share;

    public Result ValidateTransactionValueAgainstTotalQuantity(Share share, NewOrderRequestDto dto) =>
        dto.IsSellOrderRequest() && dto.Amount > share.TotalAmount ? 
            Result.Fail(ErrorType.Validation, MessageError.OperationValueGreaterThanTheTotalAssetValue) : 
            Result.Ok();
}