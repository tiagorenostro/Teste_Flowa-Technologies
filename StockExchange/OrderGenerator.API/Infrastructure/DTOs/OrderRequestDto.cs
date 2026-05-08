namespace OrderGenerator.API.Infrastructure.DTOs;

public class OrderRequestDto
{
    [Required(ErrorMessage = "Symbol is required.")]
    [MaxLength(6, ErrorMessage = "The symbol is too long. It should be a maximum of 6 characters.")]
    [RegularExpression("^[A-Z]{4}[0-9]{1}F?$", ErrorMessage = "Invalid ticker format.")]
    public string? Symbol { get; set; }
    
    [Required(ErrorMessage = "Side is required.")] 
    [AllowedValues(Constants.Side.Buy, Constants.Side.Sell, ErrorMessage = "Allowed values: B to buy and S to sell.")] 
    public char? Side { get; set; }
    
    [Required(ErrorMessage = "Amount is required.")]
    [Range(1, 100000,  ErrorMessage = "Amount must be between 1 and 100000.")]
    public int? Amount { get; set; }
    
    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, 1000,  ErrorMessage = "Price must be between 0.01 and 1000.")]
    public decimal? Price { get; set; }
}