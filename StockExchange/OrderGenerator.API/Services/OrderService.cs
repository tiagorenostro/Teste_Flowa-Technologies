namespace OrderGenerator.API.Services;

public class OrderService(IShareService shareService, 
    ITradingGateway tradingGateway, ILogger<OrderService> logger) : IOrderService
{
    private static readonly TimeOnly OpeningTime = new(hour: 10, minute: 0);
    private static readonly TimeOnly ClosingTime = new(hour: 18, minute: 0);

    public Result<OrderResponseDto> GetOrder(Guid code) =>
        InMemoryDb.Order.TryGetValue(code, out var order) ? 
            ConvertToDto(order) : 
            Error.Create(ErrorType.NotFound, MessageError.OrderNotFound, Field.Empty); 

    public IEnumerable<OrderResponseDto> GetOrders() =>
        InMemoryDb.Order.OrderByDescending(x => x.Value.OperatingDatetime)
            .Select(x => ConvertToDto(x.Value));

    public Result CreateNewOrder(NewOrderRequestDto dto) =>
        ValidateTradingHours()
            .OnSuccess(() => shareService.CreateShareIfNotExistAndValidate(dto)
                .OnSuccess(share => CreateOrder(dto)
                    .OnSuccess(order => SaveOrderAndShare(share, order)
                        .OnSuccess(() => PrepareOrderToSend(order)))));
    
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
            return Error.Create(ErrorType.Validation, MessageError.TradingOutsideOfBusinessHours, Field.Empty);

        return Result.Success();
    }

    private static Result<Order> CreateOrder(NewOrderRequestDto dto) =>
        Order.CreateOrder(dto.Symbol!, dto.Amount.GetValueOrDefault(), 
            dto.Price.GetValueOrDefault(), dto.Side.GetValueOrDefault());

    private Result PrepareOrderToSend(Order order)
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
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error.Create(ErrorType.Unexpected, MessageError.NotPossibleNewOrder, Field.Empty);
        }
    }
    
    private Result SaveOrderAndShare(Share share, Order order)
    {
        try
        {
            shareService.Save(share);
            InMemoryDb.Order[order.Code] = order;
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return Error.Create(ErrorType.Unexpected, MessageError.UnprocessedOrder, Field.Empty);
        }
    }
}