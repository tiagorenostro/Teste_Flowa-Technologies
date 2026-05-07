namespace OrderAccumulator.Worker.Infrastructure.Communication;

public class AcceptorApplication(IOrderService orderService) : MessageCracker, IApplication
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
        var resultNewOrder = orderService.NewOrder(order);
        
        var report = CreateReport(order, resultNewOrder.Value);

        SendMessage(report, sessionID);
    }

    private static ExecutionReport CreateReport(NewOrderSingle order,
        OrderDto orderDto) =>
        new(new OrderID(orderDto.OrderId.ToString()),
            new ExecID(Guid.NewGuid().ToString()),
            new ExecType(orderDto.Status == Status.Executed ? ExecType.NEW : ExecType.REJECTED),
            new OrdStatus(orderDto.Status == Status.Executed ? OrdStatus.NEW : OrdStatus.REJECTED),
            new Symbol(order.Symbol.Value),
            order.Side,
            new LeavesQty(order.OrderQty.Value),
            new CumQty(0),
            new AvgPx(0))
        {
            Symbol = order.Symbol,
            ClOrdID = order.ClOrdID,
            OrderQty = new OrderQty(orderDto.Amount),
            Price = order.Price
        };
    
    private static void SendMessage(QuickFix.Message message, SessionID sessionID) =>
        Session.SendToTarget(message, sessionID);
}