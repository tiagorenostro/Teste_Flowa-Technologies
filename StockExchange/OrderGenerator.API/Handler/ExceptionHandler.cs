namespace OrderGenerator.API.Handler;

public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, MessageError.ErrorGeneric);
            
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ErrorDto(MessageError.RequestNotProcessed, []), 
            _options, cancellationToken: cancellationToken);
            
        return true;
    }
}