namespace OrderGenerator.API.Domain;

public class Order
{
    private const int MinimumAmount = 1;
    private const int MaximumAmount = 100000;
    private const decimal MinimumPrice = 0.01M;
    private const decimal MaximumPrice = 1000;
    private const string FormatSymbolValid = "^[A-Z]{4}[0-9]{1}F?$";

    private static readonly Regex SymbolRegex = new(FormatSymbolValid, RegexOptions.Compiled);
    
    public Guid Code { get; private init; }
    public Guid CodeShare { get; private set; }
    public string Symbol { get; private init;} 
    public int Amount { get; private set; }
    public decimal Price { get; }
    public DateTime OperatingDatetime { get; private init; }
    public char Side { get; }
    public char Status { get; private set; }
    public decimal OperatingValue => Price * Amount;

    private Order(string symbol, int amount, decimal price, char side)
    {
        Code = Guid.NewGuid();
        Symbol = symbol;
        Amount = amount;
        Price = price;
        Side = side;
        Status = Constants.Status.New;
        OperatingDatetime = DateTime.UtcNow;
    }

    public bool IsOrderSell() => Side == OrderCommon.Constants.Side.Sell;
    public bool IsOrderRejected() => Status == Constants.Status.Rejected;
    public void Process(char status) => Status = status;
    public void LinkShare(Guid codeShare) => CodeShare = codeShare;

    public static Result<Order> CreateOrder(string symbol, int amount, decimal price, char side)
    {
        var errors = new List<Field>(5);
        
        if (symbol.Length is < Constants.Symbol.MinimumSymbolSize or > Constants.Symbol.MaximumSymbolSize)
            errors.Add(new Field(nameof(Symbol), MessageError.SymbolIsLong));

        if (!SymbolRegex.IsMatch(symbol))
            errors.Add(new Field(nameof(Symbol), MessageError.SymbolIsInvalid));

        if (!new[] { OrderCommon.Constants.Side.Buy, OrderCommon.Constants.Side.Sell }.Contains(side))
            errors.Add(new Field(nameof(Side), MessageError.SideValueNotAllowed));

        if (amount is < MinimumAmount or > MaximumAmount)
            errors.Add(new Field(nameof(Amount), MessageError.AmountValueNotAllowed));

        if (price is < MinimumPrice or > MaximumPrice)
            errors.Add(new Field(nameof(Price), MessageError.PriceValueNotAllowed));
        
        return errors.Count != 0 ? 
            Result<Order>.Fail(ErrorType.Validation, MessageError.UnprocessedOrder, errors) : 
            Result<Order>.Ok(new Order(symbol, amount, price, side));
    }
}