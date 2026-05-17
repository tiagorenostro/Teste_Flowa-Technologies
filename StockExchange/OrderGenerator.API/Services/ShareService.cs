namespace OrderGenerator.API.Services;

public class ShareService : IShareService
{
    public void Save(Share share) =>
        InMemoryDb.Share[share.Code] = share;
    
    public void ProcessOperation(Order order) => 
        GetBySymbol(order.Symbol)
            .ProcessOrder(order);

    public Result<ShareResponseDto> GetShare(Guid code) =>
        InMemoryDb.Share.TryGetValue(code, out var share) ? 
            Result<ShareResponseDto>.Ok(ConvertToDto(share)) : 
            Result<ShareResponseDto>.Fail(ErrorType.NotFound, MessageError.ShareNotFound);
    
    public IEnumerable<ShareResponseDto> GetShares() =>
        InMemoryDb.Share.Where(x => !x.Value.IsNoPosition())
            .Select(x => ConvertToDto(x.Value));

    public Result<Share> CreateShareIfNotExistAndValidate(NewOrderRequestDto dto)
    {
        var share = GetBySymbol(dto.Symbol!);

        if (share is not null)
            return ValidateShare(share, dto)
                .OnSuccess(() => Result<Share>.Ok(share));
        
        return dto.IsSellOrderRequest() ? 
            Result<Share>.Fail(ErrorType.Validation, MessageError.NotHavingAssetsForSale) : 
            Share.Create(dto.Symbol!);
    }
    
    private static ShareResponseDto ConvertToDto(Share share) => new(share);

    private static Share GetBySymbol(string symbol) =>
        InMemoryDb.Share.FirstOrDefault(x => x.Value.Symbol == symbol).Value;
    
    private static Result<Share> ValidateShare(Share share, NewOrderRequestDto dto) =>
        ValidateTransactionValueAgainstTotalQuantity(share, dto);

    private static Result<Share> ValidateTransactionValueAgainstTotalQuantity(Share share, NewOrderRequestDto dto) =>
        dto.IsSellOrderRequest() && dto.Amount > share.TotalAmount ? 
            Result<Share>.Fail(ErrorType.Validation, MessageError.OperationValueGreaterThanTheTotalAssetValue) : 
            Result<Share>.Ok(share);
}