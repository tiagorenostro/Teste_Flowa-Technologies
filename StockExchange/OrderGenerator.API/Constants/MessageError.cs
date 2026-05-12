namespace OrderGenerator.API.Constants;

public static class MessageError
{
    public const string RequestNotProcessed = "Your request cannot be processed.";
    public const string ErrorGeneric = "An error occured.";
    public const string UnprocessedOrder = "The order cannot be processed.";
    public const string OrderNotFound = "Order not found.";
    public const string ShareNotFound = "Share not found.";
    public const string SessionQuickFixNotFound = "Session QuickFIX not found.";
    public const string NotPossibleNewOrder = "It is not possible to place a new order.";
    public const string NotHavingAssetsForSale = "It is not possible to sell an asset that is not in your portfolio.";
    public const string OperationValueGreaterThanTheTotalAssetValue =
        "The operation value cannot exceed the total quantity of the asset.";
    public const string SymbolRequired = "Symbol is required.";
    public const string SymbolIsLong = "The symbol is too long. It should be a maximum of 6 characters.";
    public const string SymbolIsInvalid = "Invalid ticker format.";
    public const string SideRequired = "Side is required.";
    public const string SideValueNotAllowed = "Allowed values: B to Buy and S to Sell.";
    public const string AmountRequired = "Amount is required.";
    public const string AmountValueNotAllowed = "Amount must be between 1 and 100000.";
    public const string PriceRequired = "Price is required.";
    public const string PriceValueNotAllowed = "Price must be between 0.01 and 1000.";
    public const string TradingOutsideOfBusinessHours = "Our opening hours are from 10:00 to 18:00 on weekdays.";
}