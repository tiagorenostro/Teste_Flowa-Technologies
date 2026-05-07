namespace OrderAccumulator.Worker.Domains;

public class Order(int amount, decimal price, char side)
{
    public Guid OrderId { get; set; } = Guid.NewGuid();
    public int Amount { get; set; } = amount;
    public decimal Price { get; set; } = price;
    public decimal OperatingValue { get; set; } = price * amount;
    public DateTime TransactionDatetime { get; set; } = DateTime.Now;
    public char Side { get; set; } = side;
    public char Status { get; set; } = Constants.Status.New;

    public bool OrderRejected() => Status == Constants.Status.Rejected;
    public bool IsOrderSell() => Side == Constants.Side.Sell;
    public void SetStatus(bool executeOrder) =>
        Status = executeOrder ? Constants.Status.Executed : Constants.Status.Rejected;
}