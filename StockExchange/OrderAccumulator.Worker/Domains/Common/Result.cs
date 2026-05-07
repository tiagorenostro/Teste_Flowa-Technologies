namespace OrderAccumulator.Worker.Domains.Common;

public readonly struct Result<T>
{
    public T Value { get; }
    public bool Success { get; }
    public string ErrorMessage { get; }

    private Result(bool success, T value, string error)
    {
        Value = value;
        Success = success;
        ErrorMessage = error;
    }
    
    public static Result<T> Ok() => new(true, default!, null!);
    public static Result<T> Ok(T value) => new(true, value, null!);
    public static Result<T> Fail(string error) => new(false, default!, error);
    public static Result<T> Fail(T value, string error) => new(false, value, error);
    public Result<T> KeepResultOnlyWithError(Result<T> otherResult) => !Success ? this : otherResult;
}