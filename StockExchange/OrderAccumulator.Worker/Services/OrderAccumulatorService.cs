namespace OrderAccumulator.Worker.Services;

public interface IOrderAccumulatorService
{
    bool ValidateNewOrder(NewOrder newOrder);
}

public class OrderAccumulatorService : IOrderAccumulatorService
{
    private static bool ValidateValueExposure(NewOrder newOrder)
    {
        InMemoryDb.Exposures.TryGetValue(newOrder.Symbol!, out var currentExposure);
        
        switch (newOrder.Side)
        {
            case Constants.Side.Buy:
            {
                var updatedExposure = currentExposure + newOrder.TotalValue;

                if (updatedExposure > ValueExposure.LimitPerSymbol)
                    return false;

                InMemoryDb.Exposures[newOrder.Symbol!] = updatedExposure;
                break;
            }
            case Constants.Side.Sell:
                InMemoryDb.Exposures[newOrder.Symbol!] = currentExposure - newOrder.TotalValue;
                break;
        }

        return true;
    }

    public bool ValidateNewOrder(NewOrder newOrder) => 
        ValidateValueExposure(newOrder);
}