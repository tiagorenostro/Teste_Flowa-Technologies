namespace OrderGenerator.API.Routes;

public static class RouteOrder
{
    public static void AddRouteOrder(this RouteGroupBuilder routeGroupBuilder)
    {
        var routeGroup = routeGroupBuilder.MapGroup("order");

        routeGroup.MapPost("new", CreateNewOrder);
    }

    private static IResult CreateNewOrder([FromServices] IOrderService orderService, OrderRequestDto dto)
    {
        var isValid = ValidateRequest(dto, out var errorDto);
        
        if (!isValid)
            return TypedResults.BadRequest(errorDto);
        
        var createNewOrderResult = orderService.CreateNewOrder(dto);
        
        if (!createNewOrderResult.Success)
            return TypedResults.BadRequest(new ErrorDto(createNewOrderResult.ErrorMessage, []));
        
        return TypedResults.Created();
    }

    private static bool ValidateRequest(OrderRequestDto dto, out ErrorDto errorDto)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults,
            validateAllProperties: true);

        errorDto = null!;
        
        if (!isValid)
            errorDto = new ErrorDto(MessageError.UnprocessedOrder,
                Fields: validationResults.Select(x => new Field(x.MemberNames.First(), x.ErrorMessage!)));

        return isValid;
    }
}