namespace OrderGenerator.API.Infrastructure.DTOs;

public class ShareResponseDto
{
    public string? Symbol { get; set; }
    public int TotalAmount { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal FinancialExposure { get; set; }
}