namespace OrderGenerator.API.Infrastructure.Gateway;

public interface ITradingGateway
{
    Result SendOrder(OrderRequestDto dto, Guid codeOrder);
}

public class TradingGateway : ITradingGateway
{
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
    
    public Result SendOrder(OrderRequestDto dto, Guid codeOrder) =>
        _initiatorApplication.SendOrder(dto, codeOrder);

    public async Task ProcessOrderReturnAsync(OrderReportDto dto)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
            orderService.ProcessOrderReturn(dto);
        }

        await SendReturnOrderAsync(dto);
    }
    
    private async Task SendReturnOrderAsync(OrderReportDto dto) =>
        await _hubContext.Clients.All.SendAsync("OrderReportReceived", dto);
}