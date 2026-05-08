namespace OrderGenerator.API.Services;

public interface IOrderService
{
    Result CreateNewOrder(OrderRequestDto dto);
    void ProcessOrderReturn(OrderReportDto dto);
}

public class OrderService(IShareService shareService, 
    ITradingGateway tradingGateway, ILogger<OrderService> logger) : IOrderService
{
    public Result CreateNewOrder(OrderRequestDto dto)
    {
        var (createShareIfNotExistResult, share) = shareService.CreateShareIfNotExist(dto);
        
        if (!createShareIfNotExistResult.Success)
            return Result.Fail(createShareIfNotExistResult.ErrorMessage);
        
        var validateResult = shareService.ValidateTransactionValueAgainstTotalQuantity(share, dto);
        
        if (!validateResult.Success)
            return Result.Fail(validateResult.ErrorMessage);
        
        var order = new Order(dto.Symbol!, dto.Amount.GetValueOrDefault(), dto.Price.GetValueOrDefault(), dto.Side.GetValueOrDefault());
        Save(order);

        return SendNewOrder(dto, order.Code);
    }

    public void ProcessOrderReturn(OrderReportDto dto)
    {
        var order = GetByCode(dto.CodeOrder);
        order.Process(dto.Status);
        
        shareService.Process(order);
    }
    
    private static void Save(Order order) =>
        InMemoryDb.Order[order.Code] = order;
    
    private static Order GetByCode(Guid code) =>
        InMemoryDb.Order.TryGetValue(code, out var order) ? order : null!;
    
    private Result SendNewOrder(OrderRequestDto orderDto, Guid codeOrder)
    {
        try
        {
            return tradingGateway.SendOrder(orderDto, codeOrder);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Result.Fail(MessageError.NotPossibleNewOrder);
        }
    }
}