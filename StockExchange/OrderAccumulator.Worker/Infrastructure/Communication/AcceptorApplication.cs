namespace OrderAccumulator.Worker.Infrastructure.Communication;

public class AcceptorApplication(IOrderAccumulatorService orderAccumulatorService) : MessageCracker, IApplication
{
    public void OnCreate(SessionID sessionID) { }
    public void OnLogout(SessionID sessionID) { }
    public void OnLogon(SessionID sessionID)  { }
    public void ToApp(QuickFix.Message message, SessionID sessionID) { }
    public void ToAdmin(QuickFix.Message message, SessionID sessionID) { }
    public void FromAdmin(QuickFix.Message message, SessionID sessionID) { }

    public void FromApp(QuickFix.Message message, SessionID sessionID) =>
        Crack(message, sessionID);

    public void OnMessage(NewOrderSingle order, SessionID sessionID)
    {
        var isValid = orderAccumulatorService.ValidateNewOrder(ConvertNewSingleOrderToNewOrder(order));
        
        var report = CreateReport(order, isValid);

        SendMessage(report, sessionID);
    }

    private static ExecutionReport CreateReport(NewOrderSingle order, bool isValid) =>
        new(new OrderID(Guid.NewGuid().ToString()),
            new ExecID(Guid.NewGuid().ToString()),
            new ExecType(isValid ? ExecType.NEW : ExecType.REJECTED),
            new OrdStatus(isValid ? OrdStatus.NEW : OrdStatus.REJECTED),
            new Symbol(order.Symbol.Value),
            order.Side,
            new LeavesQty(order.OrderQty.Value),
            new CumQty(0),
            new AvgPx(0))
        {
            Symbol = order.Symbol,
            ClOrdID = order.ClOrdID,
            OrderQty = new OrderQty(order.OrderQty.Value),
            Price = order.Price
        };
    
    private static void SendMessage(QuickFix.Message message, SessionID sessionID) =>
        Session.SendToTarget(message, sessionID);

    private static NewOrder ConvertNewSingleOrderToNewOrder(NewOrderSingle order) =>
        new(order.Symbol.Value, DefineSide(order.Side), TotalValue: order.OrderQty.Value * order.Price.Value);

    private static char DefineSide(QuickFix.Fields.Side side) =>
        side.Value switch
        {
            QuickFix.Fields.Side.BUY => Constants.Side.Buy,
            QuickFix.Fields.Side.SELL => Constants.Side.Sell,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, "Unmapped side.")
        };
}