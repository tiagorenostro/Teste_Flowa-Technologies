namespace OrderGenerator.API.Domain.Common;

public readonly struct Result
{
    public bool Success { get; }
    public Error Error { get; }
    
    private Result(bool success, Error error)
    {
        Success = success;
        Error = error;
    }
    
    public static Result Ok() => new(true, default);
    public static Result Fail(Error error) => new(false, error);
    public static Result Fail(ErrorType errorType, string message) => new(false, new Error(errorType, message, []));
    public Result OnSuccess(Func<Result> next) => !Success ? this : next();
}

public readonly struct Result<T>
{
    public T Value { get; }
    public bool Success { get; }
    public Error Error { get; }

    private Result(bool success, T value, Error error)
    {
        Success = success;
        Value = value;
        Error = error;
    }
    
    public static Result<T> Ok(T value) => new(true, value, default);
    public static Result<T> Fail(ErrorType errorType, string messageError) => 
        new(false, default!, new Error(errorType, messageError, []));
    public static Result<T> Fail(ErrorType errorType, string messageError, IEnumerable<Field> fields) => 
        new(false, default!, new Error(errorType, messageError, fields));
    public static Result<T> Fail(Error error) => new(false, default!, error);
    public Result OnSuccess(Func<T, Result> next) => !Success ? Result.Fail(Error) : next(Value);
    public Result<T> OnSuccess(Func<Result<T>> next) => !Success ? Fail(Error) : next();
}

public readonly struct Error(ErrorType errorType, string message, IEnumerable<Field> fields)
{
    public ErrorType ErrorType { get; } = errorType;
    public string Message { get; } = message;
    public IEnumerable<Field> Fields { get; } = fields;
}

public readonly struct Field(string name, string message)
{
    public string Name { get; } = name;
    public string Message { get; } = message;
}