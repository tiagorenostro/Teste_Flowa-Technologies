namespace OrderAccumulator.Worker.Services;

public class OrderService(IShareRepository shareRepository) : IOrderService
{
    private static Order CreateNewOrder(NewOrderSingle order)
    {
        var side = order.Side.Value switch
        {
            QuickFix.Fields.Side.BUY => Constants.Side.Buy,
            QuickFix.Fields.Side.SELL => Constants.Side.Sell
        };
        
        return new Order((int)order.OrderQty.Value, order.Price.Value, side);
    }
    
    private Share GetOrAdd(string symbol) => shareRepository.GetOrAdd(symbol);
    private bool ExistShareExecuted(string symbol) => shareRepository.ExistShareExecuted(symbol);

    private Result<bool> ValidateOrderSell(NewOrderSingle newOrder)
    {
        if (newOrder.Side.Value != QuickFix.Fields.Side.SELL)
            return Result<bool>.Ok();
        
        return !ExistShareExecuted(newOrder.Symbol.Value) ? 
            Result<bool>.Fail("Share not found") : 
            Result<bool>.Ok();
    }

    public Result<OrderDto> NewOrder(NewOrderSingle order)
    {
        var share = GetOrAdd(order.Symbol.Value);
        var newOrder = CreateNewOrder(order);
        var orderDto = new OrderDto(newOrder.OrderId);
        
        lock (share)
        {
            var overallResult = share.ValidateFinancialExposure(newOrder, out var valueFinancialExposure)
                .KeepResultOnlyWithError(ValidateOrderSell(order));
            
            share.AddNewOrder(newOrder, valueFinancialExposure, overallResult.Success);
            
            orderDto.SetStatus(newOrder.Status);
            orderDto.SetAmount(newOrder.Amount);

            if (!overallResult.Success)
                return Result<OrderDto>.Fail(orderDto, overallResult.ErrorMessage);
        }
        
        return Result<OrderDto>.Ok(orderDto);
    }
}