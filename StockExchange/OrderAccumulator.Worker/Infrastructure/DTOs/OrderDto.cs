namespace OrderAccumulator.Worker.Infrastructure.DTOs;

public class OrderDto(Guid orderId)
{
    public Guid OrderId { get; set; } = orderId;
    public char Status { get; set; }
    public int Amount { get; set; }
    
    public void SetStatus(char status) => Status = status;
    public void SetAmount(int amount) => Amount = amount;
}