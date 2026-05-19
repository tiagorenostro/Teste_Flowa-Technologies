namespace OrderGenerator.API.Infrastructure.Gateway;

public interface ITradingGateway
{
    Result SendOrder(Order order);
}

public class TradingGateway : ITradingGateway
{
    private const string Method = "OrderReportReceived";
    
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IInitiatorApplication _initiatorApplication;
    
    public TradingGateway(IInitiatorApplication initiatorApplication,
        IHubContext<TradingHub> hubContext,
        IServiceScopeFactory serviceScopeFactory)
    {
        _hubContext = hubContext;
        _initiatorApplication = initiatorApplication;
        _serviceScopeFactory = serviceScopeFactory;

        _initiatorApplication.OnProcessOrderReportReceived += async dto =>
            await ProcessOrderReturnAsync(dto);
    }
    
    public Result SendOrder(Order order) =>
        _initiatorApplication.SendOrder(order);

    private async Task ProcessOrderReturnAsync(OrderReportDto dto)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IOrderService>()
                .ProcessOrderReturn(dto);
        }

        await SendReturnOrderAsync(dto);
    }
    
    private async Task SendReturnOrderAsync(OrderReportDto dto) =>
        await _hubContext.Clients.All.SendAsync(Method, dto);
}