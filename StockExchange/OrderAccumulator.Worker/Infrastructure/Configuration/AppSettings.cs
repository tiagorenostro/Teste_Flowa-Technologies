namespace OrderAccumulator.Worker.Infrastructure.Configuration;

public sealed record AppSettings(string Role, string PathFileConfigurationQuickFIX)
{
    public bool IsAcceptor() => string.Equals(Role, OrderCommon.Constant.Role.Acceptor, StringComparison.OrdinalIgnoreCase);
}