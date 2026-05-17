namespace OrderGenerator.API.Messagings.DTOs;

public sealed record ShareResponseDto
{
    public Guid Code { get; init; }
    public string? Symbol { get; init; } 
    public int TotalAmount { get; init; }
    public decimal AveragePrice { get; init; }
    public decimal FinancialExposure { get; init; }
    public IEnumerable<Guid> OrderCodes { get; init; }

    public ShareResponseDto(Share share)
    {
        Code = share.Code;
        Symbol = share.Symbol;
        TotalAmount = share.TotalAmount;
        AveragePrice = share.AveragePrice;
        FinancialExposure = share.FinancialExposure;
        OrderCodes = share.OrderCodes;
    }
}