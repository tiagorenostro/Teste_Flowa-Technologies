namespace OrderGenerator.API.Infrastructure.Communication;

public interface IInitiatorApplication : IApplication
{
    event Action<OrderReportDto> OnProcessOrderReportReceived;
    Result SendOrder(Order order);
}

public class InitiatorApplication : MessageCracker, IInitiatorApplication
{
    private Session Session { get; set; }
    private bool IsConnected { get; set; }
    
    public event Action<OrderReportDto> OnProcessOrderReportReceived;
    
    public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void ToApp(QuickFix.Message message, SessionID sessionID) { }

    public void OnLogon(SessionID sessionID) => IsConnected = true;
    public void OnLogout(SessionID sessionID) => IsConnected = false;
    public void FromApp(QuickFix.Message message, SessionID sessionID) => Crack(message, sessionID);

    public void OnCreate(SessionID sessionID)
    {
        Session = Session.LookupSession(sessionID)!;
        
        if (Session is null)
            throw new ApplicationException(MessageError.SessionQuickFixNotFound);
    }

    public void OnMessage(ExecutionReport report, SessionID sessionID) =>
        OnProcessOrderReportReceived.Invoke(CreateOrderReport(report));

    public Result SendOrder(Order order)
    {
        if (!IsConnected) 
            return Result.Fail(ErrorType.ExternalError, MessageError.NotPossibleNewOrder);
        
        var send = Session.SendToTarget(PrepareShipmentOrder(order), Session.SessionID);
            
        return send ? Result.Ok() : Result.Fail(ErrorType.ExternalError, MessageError.NotPossibleNewOrder);
    }
    
    private static NewOrderSingle PrepareShipmentOrder(Order order)
    {
        var newOrderSingle = new NewOrderSingle(
            new ClOrdID(order.Code.ToString()),
            new QuickFix.Fields.Symbol(order.Symbol),
            PrepareSide(order.Side),
            new TransactTime(DateTime.Now),
            new OrdType(OrdType.MARKET));
                
        newOrderSingle.Set(new HandlInst(HandlInst.MANUAL_ORDER));
        newOrderSingle.Set(new OrderQty(order.Amount));
        newOrderSingle.Set(new TimeInForce(TimeInForce.DAY));
        newOrderSingle.Set(new Price(order.Price));
        
        return newOrderSingle;
    }

    private static QuickFix.Fields.Side PrepareSide(char side) =>
        side switch
        {
            Constants.Side.Buy => new QuickFix.Fields.Side(QuickFix.Fields.Side.BUY),
            Constants.Side.Sell => new QuickFix.Fields.Side(QuickFix.Fields.Side.SELL),
            _ => new QuickFix.Fields.Side()
        };

    private static OrderReportDto CreateOrderReport(ExecutionReport report) =>
        new(Guid.Parse(report.ClOrdID.Value),
            report.IsSetSymbol() ? report.Symbol.Value : string.Empty,
            report.IsSetOrderQty() ? report.OrderQty.Value : 0,
            report.IsSetPrice() ? report.Price.Value : 0,
            DefineStatus(report),
            DefineSide(report));

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