namespace OrderAccumulator.Worker.Domain;

public sealed record NewOrder(string? Symbol, char Side, decimal TotalValue)
{
    public bool IsOrderBuy() => Side == OrderCommon.Constants.Side.Buy;
}