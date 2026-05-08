namespace OrderGenerator.API.Domain;

public class Share(string? symbol)
{
    public string? Symbol { get; set; } = symbol;
    public int TotalAmount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal FinancialExposure { get; set; }
    public ICollection<Guid> CodeOrders { get; set; } = [];

    public void ProcessOrder(Order order)
    {
        if (order.OrderRejected())
        {
            AddOrder(order);
            return;
        }
        
        CalculateFinancialExposure(order);
        
        if (order.IsOrderSell())
        {
            AddSalesOrder(order);
            return;
        }
        
        AddPurchaseOrder(order);
        CalculateAveragePrice();
    }
    
    private void CalculateFinancialExposure(Order order)
    {
        var modifier = order.Side switch
        {
            Constants.Side.Sell => -1,
            Constants.Side.Buy => 1,
            _ => 0
        };
        
        var operatingValueModifier = order.OperatingValue * modifier;
        FinancialExposure += operatingValueModifier;
    }

    private void AddPurchaseOrder(Order order)
    {
        TotalAmount += order.Amount;
        AddOrder(order);
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

    private void AddOrder(Order order) => CodeOrders.Add(order.Code);

    private void CalculateAveragePrice() =>   
        AveragePrice = FinancialExposure / TotalAmount;
}