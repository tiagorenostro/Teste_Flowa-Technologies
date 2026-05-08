namespace OrderGenerator.API.Constants;

public static class MessageError
{
    public const string RequestNotProcessed = "Your request cannot be processed.";
    public const string ErrorGeneric = "An error occured.";
    public const string UnprocessedOrder = "The order cannot be processed.";
    public const string SessionQuickFIXNotFound = "Session QuickFIX not found.";
    public const string NotPossibleNewOrder = "It is not possible to place a new order.";
}