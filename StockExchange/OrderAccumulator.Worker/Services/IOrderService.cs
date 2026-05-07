using OrderAccumulator.Worker.Domains.Common;
using OrderAccumulator.Worker.Infrastructure.DTOs;

namespace OrderAccumulator.Worker.Services;

public interface IOrderService
{
    Result<OrderDto> NewOrder(NewOrderSingle order);
}