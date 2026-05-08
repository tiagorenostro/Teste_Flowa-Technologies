namespace OrderGenerator.API.Domain.Common;

public readonly struct Result
{
    public bool Success { get; }
    public string ErrorMessage { get; }

    private Result(bool success, string error)
    {
        Success = success;
        ErrorMessage = error;
    }
    
    public static Result Ok() => new(true, string.Empty);
    public static Result Fail(string error) => new(false, error);
}