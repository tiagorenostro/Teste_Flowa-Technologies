namespace OrderGenerator.API.Messagings.DTOs;

public sealed record NewOrderRequestDto(
    [Required(ErrorMessage = MessageError.SymbolRequired)] string? Symbol,
    [Required(ErrorMessage = MessageError.SideRequired)] char? Side,
    [Required(ErrorMessage = MessageError.AmountRequired)] int? Amount,
    [Required(ErrorMessage = MessageError.PriceRequired)] decimal? Price)
{
    public bool IsSellOrderRequest() => Side == OrderCommon.Constants.Side.Sell;
}