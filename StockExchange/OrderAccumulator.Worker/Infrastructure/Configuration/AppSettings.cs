namespace OrderAccumulator.Worker.Infrastructure.Configuration;

public sealed record AppSettings(string PathFileConfigurationQuickFIX, string Role)
{
    public bool IsAcceptor() => string.Equals(Role, "Acceptor", StringComparison.OrdinalIgnoreCase);
}