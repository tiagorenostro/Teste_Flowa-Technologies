namespace OrderGenerator.API.Domain;

public class Order(string symbol, int amount, decimal price, char side)
{
    public Guid Code { get; set; } = Guid.NewGuid();
    public string Symbol { get; set; } = symbol;
    public int Amount { get; set; } = amount;
    public decimal Price { get; set; } = price;
    public decimal OperatingValue { get; set; } = price * amount;
    public DateTime TransactionDatetime { get; set; } = DateTime.Now;
    public char Side { get; set; } = side;
    public char Status { get; set; } = Constants.Status.New;

    public bool OrderRejected() => Status == Constants.Status.Rejected;
    public bool IsOrderSell() => Side == Constants.Side.Sell;
    public void SetStatus(char status) => Status = status;
    public void Process(char status) => SetStatus(status);
}