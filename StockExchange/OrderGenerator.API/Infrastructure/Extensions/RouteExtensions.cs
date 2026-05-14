namespace OrderGenerator.API.Infrastructure.Extensions;

public static class RouteExtensions
{
    public static IResult ToHttpResponse<T>(this T data) => TypedResults.Ok(data);
    public static IResult ToHttpResponse<T>(this Result<T> result) => 
        result.Success ? 
            TypedResults.Ok(result.Value) : 
            MapError(result.Error);

    public static IResult ToCreatedResponse(this Result result) =>
        result.Success ? 
            TypedResults.Created() : 
            MapError(result.Error);
    
    public static bool IsInvalid<T>(this T dto, out IResult? errorResponse) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), validationResults, true);

        if (isValid)
        {
            errorResponse = null;
            return false;
        }

        var errorDto = new ErrorDto(MessageError.UnprocessedOrder,
            validationResults.Select(x => new FieldDto(x.MemberNames.First(), x.ErrorMessage!)));

        errorResponse = TypedResults.BadRequest(errorDto);
        return true;
    }
    
    private static IResult MapError(Error error)
    {
        var errorDto = new ErrorDto(error.Message, error.Fields.Select(x => new FieldDto(x.Name, x.Message)));

        return error.ErrorType switch
        {
            ErrorType.NotFound => TypedResults.NotFound(errorDto),
            ErrorType.Validation => TypedResults.BadRequest(errorDto),
            ErrorType.Unexpected => TypedResults.InternalServerError(errorDto),
            ErrorType.ExternalError => TypedResults.Json(errorDto, statusCode: (int)HttpStatusCode.BadGateway),
            _ => TypedResults.UnprocessableEntity(errorDto)
        };
    }
}