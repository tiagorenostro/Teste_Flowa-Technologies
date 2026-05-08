namespace OrderAccumulator.Worker.Domain;

public record NewOrder(string? Symbol, char Side, decimal TotalValue);