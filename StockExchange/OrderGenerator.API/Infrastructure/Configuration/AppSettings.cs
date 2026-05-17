namespace OrderGenerator.API.Infrastructure.Configuration;

public sealed record AppSettings(
    string? PatternHub,
    string? CorsPolicy,
    string? UrlStockExhangeWeb,
    string? Role,
    string? PathFileConfigurationQuickFIX)
{
    public bool IsAcceptor() => string.Equals(Role, "Acceptor", StringComparison.OrdinalIgnoreCase);
}