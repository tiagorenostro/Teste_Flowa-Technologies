namespace OrderGenerator.API.Domain;

public partial class Order
{
    private const int MinimumAmount = 1;
    private const int MaximumAmount = 100000;
    private const decimal MinimumPrice = 0.01M;
    private const decimal MaximumPrice = 1000;
    private const string FormatSymbolValid = "^[A-Z]{4}[0-9]{1}F?$";
    
    public Guid Code { get; private init; }
    public Guid CodeShare { get; private set; }
    public string Symbol { get; private init;} 
    public int Amount { get; }
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
        Status = Constant.Status.New;
        OperatingDatetime = DateTime.UtcNow;
    }

    public bool IsOrderSell() => Side == OrderCommon.Constant.Side.Sell;
    public bool IsOrderRejected() => Status == Constant.Status.Rejected;
    public void Process(char status) => Status = status;
    public void LinkShare(Guid codeShare) => CodeShare = codeShare;

    public static Result<Order> CreateOrder(string symbol, int amount, decimal price, char side)
    {
        List<Field> fields = null!;
        
        if (symbol.Length is < Constant.Symbol.MinimumSymbolSize or > Constant .Symbol.MaximumSymbolSize)
            (fields ??= []).Add(Field.Create(nameof(symbol), MessageError.SymbolIsLong));
        else if (!SymbolRegex().IsMatch(symbol))
            (fields ??= []).Add(Field.Create(nameof(symbol), MessageError.SymbolIsInvalid));

        if (side != OrderCommon.Constant.Side.Buy && side != OrderCommon.Constant.Side.Sell)
            (fields ??= []).Add(Field.Create(nameof(side), MessageError.SideValueNotAllowed));

        if (amount is < MinimumAmount or > MaximumAmount)
            (fields ??= []).Add(Field.Create(nameof(amount), MessageError.AmountValueNotAllowed));

        if (price is < MinimumPrice or > MaximumPrice)
            (fields ??= []).Add(Field.Create(nameof(price), MessageError.PriceValueNotAllowed));

        if (fields is not null)
            return Error.Create(ErrorType.Validation, MessageError.UnprocessedOrder, fields);
        
        return new Order(symbol, amount, price, side);
    }

    [GeneratedRegex(pattern: FormatSymbolValid, RegexOptions.Compiled)]
    private static partial Regex SymbolRegex();
}