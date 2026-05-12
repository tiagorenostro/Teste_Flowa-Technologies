namespace OrderGenerator.API.Services;

public interface IOrderService
{
    Result<OrderResponseDto> GetOrder(Guid code);
    IEnumerable<OrderResponseDto> GetOrders();
    Result CreateNewOrder(NewOrderRequestDto dto);
    void ProcessOrderReturn(OrderReportDto dto);
}

public class OrderService(IShareService shareService, 
    ITradingGateway tradingGateway, ILogger<OrderService> logger) : IOrderService
{
    private static readonly TimeOnly OpeningTime = new(10, 0);
    private static readonly TimeOnly ClosingTime = new(18, 0);

    public Result<OrderResponseDto> GetOrder(Guid code) =>
        InMemoryDb.Order.TryGetValue(code, out var order) ? 
            Result<OrderResponseDto>.Ok(ConvertToDto(order)) : 
            Result<OrderResponseDto>.Fail(ErrorType.NotFound, MessageError.OrderNotFound); 

    public IEnumerable<OrderResponseDto> GetOrders() =>
        InMemoryDb.Order.OrderByDescending(x => x.Value.OperatingDatetime)
            .Select(x => ConvertToDto(x.Value));

    public Result CreateNewOrder(NewOrderRequestDto dto) =>
        ValidateTradingHours()
            .OnSuccess(() => shareService.CreateShareIfNotExist(dto))
            .OnSuccess(share => 
                shareService.ValidateTransactionValueAgainstTotalQuantity(share, dto)
                    .OnSuccess(() => CreateOrder(dto))
                    .OnSuccess(order => SaveOrderAndShare(share, order)
                        .OnSuccess(() => PrepareToSend(order))));
    
    public void ProcessOrderReturn(OrderReportDto dto)
    {
        var order = GetByCode(dto.CodeOrder);
        order.Process(dto.Status);
        
        shareService.ProcessOperation(order);
    }

    private static OrderResponseDto ConvertToDto(Order order) => new(order);
    
    private static Order GetByCode(Guid code) =>
        InMemoryDb.Order.TryGetValue(code, out var order) ? order : null!;
    
    private static Result ValidateTradingHours()
    {
        var now = DateTime.Now;
        var timeOnly = TimeOnly.FromDateTime(now);
        
        if (now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday || 
            timeOnly < OpeningTime || timeOnly > ClosingTime)
            return Result.Fail(ErrorType.Validation, MessageError.TradingOutsideOfBusinessHours);

        return Result.Ok();
    }

    private Result SaveOrderAndShare(Share share, Order order)
    {
        try
        {
            shareService.Save(share);
            InMemoryDb.Order[order.Code] = order;
            return Result.Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Result.Fail(ErrorType.Unexpected, MessageError.UnprocessedOrder);
        }
    }

    private static Result<Order> CreateOrder(NewOrderRequestDto dto)
    {
        var newOrderResult = Order.CreateOrder(dto.Symbol!, dto.Amount.GetValueOrDefault(), 
            dto.Price.GetValueOrDefault(), dto.Side.GetValueOrDefault());
        
        return !newOrderResult.Success ? Result<Order>.Fail(newOrderResult.Error) : newOrderResult;
    }

    private Result PrepareToSend(Order order)
    {
        order.Process(Status.Liquidation);
        
        return SendNewOrder(order);
    }
    
    private Result SendNewOrder(Order order)
    {
        try
        {
            return tradingGateway.SendOrder(order);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            return Result.Fail(ErrorType.Unexpected, MessageError.NotPossibleNewOrder);
        }
    }
}