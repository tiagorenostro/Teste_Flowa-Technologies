namespace OrderGenerator.API.Infrastructure.Gateway;

public interface ITradingGateway
{
    bool SendOrder(OrderRequestDto orderRequestDto);
}

public class TradingGateway : ITradingGateway
{
    private readonly IInitiatorApplication _initiatorApplication;

    public TradingGateway(IInitiatorApplication initiatorApplication, IHubContext<TradingHub> hubContext)
    {
        _initiatorApplication = initiatorApplication;
        
        _initiatorApplication.OnOrderReportReceived += async dto => 
            await hubContext.Clients.All.SendAsync("OrderReportReceived", dto);
    }

    public bool SendOrder(OrderRequestDto orderRequestDto) =>
        _initiatorApplication.SendOrder(orderRequestDto);
}