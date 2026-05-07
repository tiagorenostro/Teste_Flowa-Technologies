using OrderAccumulator.Worker.Domains;

namespace OrderAccumulator.Worker.Infrastructure.Repository;

public interface IShareRepository
{
    Share GetOrAdd(string symbol);
    bool ExistShareExecuted(string symbol);
}