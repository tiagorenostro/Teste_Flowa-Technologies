namespace OrderGenerator.API.Messagings.DTOs;

public sealed record OrderResponseDto
{
    public Guid Code { get; init; }
    public string Symbol { get; init; }
    public DateTime OperatingDatetime { get; init; }
    public char Side { get; init; }
    public decimal Price { get; init; }
    public decimal Amount { get; init; }
    public decimal OperatingValue { get; init; }
    public char Status { get; init; }

    public OrderResponseDto(Order order)
    {
        Code = order.Code;
        Symbol = order.Symbol;
        OperatingDatetime = order.OperatingDatetime;
        Side = order.Side;
        Price = order.Price;
        Amount = order.Amount;
        OperatingValue = order.OperatingValue;
        Status = order.Status;
    }
}