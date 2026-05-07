namespace OrderAccumulator.Worker.Domains;

public class Share(string? symbol)
{
    public string? Symbol { get; set; } = symbol;
    public int TotalAmount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal FinancialExposure { get; set; }
    public ICollection<Order> Orders { get; set; } = [];

    public Result<bool> ValidateFinancialExposure(Order order, out decimal valueFinancialExposure)
    {
        var modifier = order.Side switch
        {
            Constants.Side.Sell => -1,
            Constants.Side.Buy => 1,
            _ => 0
        };
        
        var operatingValueModifier = order.OperatingValue * modifier;
        valueFinancialExposure = FinancialExposure + operatingValueModifier;
        
        return valueFinancialExposure > ValueExposure.LimitPerSymbol ? 
            Result<bool>.Fail("Financial exposure exceeds limit.") : 
            Result<bool>.Ok();
    }

    public void AddNewOrder(Order order, decimal operatingValue, bool executeOrder)
    {
        order.SetStatus(executeOrder);

        if (order.OrderRejected())
        {
            AddOrder(order);
            return;
        }
        
        SetFinancialExposure(operatingValue);
        
        if (order.IsOrderSell())
        {
            AddSalesOrder(order);
            return;
        }
        
        AddPurchaseOrder(order);
    }

    private void AddPurchaseOrder(Order order)
    {
        TotalAmount += order.Amount;
        AddOrder(order);
        CalculateAveragePrice(order);
    }

    private void AddSalesOrder(Order order)
    {
        if (order.Amount > TotalAmount)
            order.Amount = TotalAmount;
        
        TotalAmount -= order.Amount;

        if (TotalAmount == 0)
            AveragePrice = 0;
            
        AddOrder(order);
    }

    private void AddOrder(Order order) => Orders.Add(order);

    private void CalculateAveragePrice(Order order)
    {
        if (order.IsOrderSell())
            return;
        
        var totalValueOperations = Orders.Where(x => x.Status == Status.Executed)
            .Sum(o => o.OperatingValue);
        
        AveragePrice = totalValueOperations / TotalAmount;
    }

    private void SetFinancialExposure(decimal operatingValue) => FinancialExposure = operatingValue;
}