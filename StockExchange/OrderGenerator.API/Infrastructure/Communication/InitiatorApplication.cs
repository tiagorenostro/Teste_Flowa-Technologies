namespace OrderGenerator.API.Infrastructure.Communication;

public interface IInitiatorApplication : IApplication
{
    event Action<OrderReportDto> OnOrderReportReceived;
    bool SendOrder(OrderRequestDto orderDto);
}

public class InitiatorApplication : MessageCracker, IInitiatorApplication
{
    private Session Session { get; set; }
    private bool IsConnected { get; set; }
    
    public event Action<OrderReportDto> OnOrderReportReceived;
    
    public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void ToApp(QuickFix.Message message, SessionID sessionID) { }

    public void OnLogon(SessionID sessionID) { IsConnected = true; }
    public void OnLogout(SessionID sessionID) { IsConnected = false; }
    
    public void FromApp(QuickFix.Message message, SessionID sessionID) =>
        Crack(message, sessionID);

    public void OnCreate(SessionID sessionID)
    {
        Session = Session.LookupSession(sessionID)!;
        
        if (Session is null)
            throw new ApplicationException("Session QuickFIX not found.");
    }

    public void OnMessage(ExecutionReport report, SessionID sessionID) =>
        OnOrderReportReceived.Invoke(ConvertReport(report));

    public bool SendOrder(OrderRequestDto orderDto) =>
        IsConnected
            ? Session.SendToTarget(PrepareShipmentOrder(orderDto), Session.SessionID)
            : throw new ApplicationException("It is not possible to place a new order.");
    
    private static NewOrderSingle PrepareShipmentOrder(OrderRequestDto orderRequestDto)
    {
        var order = new NewOrderSingle(
            new ClOrdID(Guid.NewGuid().ToString()),
            new Symbol(orderRequestDto.Symbol!),
            PrepareSide(orderRequestDto),
            new TransactTime(DateTime.Now),
            new OrdType(OrdType.MARKET));
                
        order.Set(new HandlInst(HandlInst.MANUAL_ORDER));
        order.Set(new OrderQty(orderRequestDto.Amount.GetValueOrDefault()));
        order.Set(new TimeInForce(TimeInForce.DAY));
        order.Set(new Price(orderRequestDto.Price.GetValueOrDefault()));
        
        return order;
    }

    private static QuickFix.Fields.Side PrepareSide(OrderRequestDto orderRequestDto) =>
        orderRequestDto.Side switch
        {
            Constants.Side.Buy => new QuickFix.Fields.Side(QuickFix.Fields.Side.BUY),
            Constants.Side.Sell => new QuickFix.Fields.Side(QuickFix.Fields.Side.SELL),
            _ => new QuickFix.Fields.Side()
        };

    private static OrderReportDto ConvertReport(ExecutionReport report) =>
        new()
        {
            Amount = report.IsSetOrderQty() ? report.OrderQty.Value : 0,
            Symbol = report.IsSetSymbol() ? report.Symbol.Value : string.Empty,
            Price = report.IsSetPrice() ? report.Price.Value : 0,
            Status = DefineStatus(report),
            Side = DefineSide(report)
        };

    private static char DefineStatus(ExecutionReport report) =>
        report.OrdStatus.Value switch
        {
            ExecType.NEW => Status.Executed,
            ExecType.REJECTED => Status.Rejected,
            _ => Status.Outher
        };
    
    private static char DefineSide(ExecutionReport report) =>
        report.Side.Value switch
        {
            QuickFix.Fields.Side.BUY => Constants.Side.Buy,
            QuickFix.Fields.Side.SELL => Constants.Side.Sell
        };
}