namespace OrderGenerator.API.Infrastructure.Configuration;

public sealed record AppSettings(string? PatternHub,
    string? CorsPolicy, 
    string? UrlStockExhangeWeb,
    string? PathFileConfigurationQuickFIX);