namespace OrderGenerator.API.Infrastructure.DTOs;

public class OrderReportDto
{
    public Guid CodeOrder { get; set; }
    public string? Symbol { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public char Status { get; set; }
    public char Side { get; set; }
}