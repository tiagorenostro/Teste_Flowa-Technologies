namespace OrderGenerator.API.Infrastructure.Communication;

public interface IInitiatorApplication : IApplication
{
    event Action<OrderReportDto> OnProcessOrderReportReceived;
    Result SendOrder(OrderRequestDto orderDto, Guid codeOrder);
}

public class InitiatorApplication : MessageCracker, IInitiatorApplication
{
    private Session Session { get; set; }
    private bool IsConnected { get; set; }
    
    public event Action<OrderReportDto> OnProcessOrderReportReceived;
    
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
            throw new ApplicationException(MessageError.SessionQuickFIXNotFound);
    }

    public void OnMessage(ExecutionReport report, SessionID sessionID) =>
        OnProcessOrderReportReceived.Invoke(ConvertReport(report));

    public Result SendOrder(OrderRequestDto orderDto, Guid codeOrder)
    {
        if (!IsConnected) 
            return Result.Fail(MessageError.NotPossibleNewOrder);
        
        var send = Session.SendToTarget(PrepareShipmentOrder(orderDto, codeOrder), Session.SessionID);
            
        return send ? Result.Ok() : Result.Fail(MessageError.NotPossibleNewOrder);
    }
       
    
    private static NewOrderSingle PrepareShipmentOrder(OrderRequestDto orderRequestDto, Guid codeOrder)
    {
        var order = new NewOrderSingle(
            new ClOrdID(codeOrder.ToString()),
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
            CodeOrder = Guid.Parse(report.ClOrdID.Value),
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