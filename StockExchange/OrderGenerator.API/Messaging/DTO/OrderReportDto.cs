namespace OrderGenerator.API.Messaging.DTO;

public sealed record OrderReportDto(Guid CodeOrder,
    string? Symbol,
    decimal Amount,
    decimal Price,
    char Status,
    char Side);