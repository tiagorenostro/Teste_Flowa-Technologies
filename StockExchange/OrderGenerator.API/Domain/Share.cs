namespace OrderGenerator.API.Domain;

public class Share
{
    private readonly List<Guid> _orderCodes = [];
    
    public Guid Code { get; }
    public string? Symbol { get; private set; }
    public int TotalAmount { get; private set; }
    public decimal AveragePrice { get; private set; }
    public decimal FinancialExposure { get; private set; }
    public char Status { get; private set; }
    public IReadOnlyCollection<Guid> OrderCodes => _orderCodes;

    private Share(string symbol)
    {
        Code = Guid.NewGuid();
        Symbol = symbol;
        
        UpdateStatus();
    }
    
    private void UpdateStatus() =>
        Status = TotalAmount switch
        {
            0 => Constants.Status.Flat,
            > 0 => Constants.Status.Long,
            < 0 => Constants.Status.Short
        };
    
    private void AddOrderReference(Order order)
    {
        if (!_orderCodes.Contains(order.Code))
            _orderCodes.Add(order.Code);
    }

    private void ProcessPurchase(Order order)
    {
        TotalAmount += order.Amount;
        var totalCost = TotalAmount * AveragePrice + order.Amount * order.Price;
        
        if (TotalAmount > 0)
            AveragePrice = totalCost / TotalAmount;
        
        FinancialExposure += order.OperatingValue;
    }

    private void ProcessSale(Order order)
    {
        var amountToSell = Math.Min(order.Amount, TotalAmount);
        
        TotalAmount -= amountToSell;
        FinancialExposure -= amountToSell * order.Price;

        if (TotalAmount != 0) 
            return;
        
        AveragePrice = 0;
        FinancialExposure = 0;
    }
    
    public bool IsNoPosition() => Status == Constants.Status.Flat;
    
    public void ProcessOrder(Order order)
    {
        order.LinkShare(Code);
        
        if (order.IsOrderRejected())
        {
            AddOrderReference(order);
            return;
        }
        
        if (order.IsOrderSell())
            ProcessSale(order);
        else
            ProcessPurchase(order);
        
        UpdateStatus();
        AddOrderReference(order);
    }

    public static Result<Share> Create(string? symbol)
    {
        if (symbol!.Length is < Constants.Symbol.MinimumSymbolSize or > Constants.Symbol.MaximumSymbolSize)
            return Result<Share>.Fail(new Error(ErrorType.Validation, MessageError.UnprocessedOrder,
                fields: [new Field(nameof(Symbol), MessageError.SymbolIsLong)]));
        
        return Result<Share>.Ok(new Share(symbol));
    }
}