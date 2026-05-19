namespace OrderGenerator.API.Domain.Common;

public readonly struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    
    private Result(bool success, Error error)
    {
        IsSuccess = success;
        Error = error;
    }
    
    public static implicit operator Result(Error error) => Fail(error);
    
    public static Result Success() => new(true, default);
    public static Result Fail(Error error) => new(false, error);
    public Result OnSuccess(Func<Result> next) => !IsSuccess ? this : next();
}

public readonly struct Result<T>
{
    public T Value { get; }
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(bool isSuccess, T value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Fail(error);

    private static Result<T> Success(T value) => new(true, value, default);
    private static Result<T> Fail(Error error) => new(false, default!, error);
    public Result OnSuccess(Func<T, Result> next) => !IsSuccess ? Result.Fail(Error) : next(Value);
    public Result<T> OnSuccess(Func<Result<T>> next) => !IsSuccess ? Fail(Error) : next();
}

public readonly struct Error(ErrorType errorType, string message, IEnumerable<Field> fields)
{
    public ErrorType ErrorType { get; } = errorType;
    public string Message { get; } = message;
    public IEnumerable<Field> Fields { get; } = fields;
    
    public static Error Create(ErrorType errorType, string message, IEnumerable<Field> fields) => 
        new(errorType, message, fields);
}

public readonly struct Field(string name, string message)
{
    public string Name { get; } = name;
    public string Message { get; } = message;
    
    public static IEnumerable<Field> Empty => [];
    
    public static Field Create(string name, string message) => new(name, message);
    public static IEnumerable<Field> CreateCollection(string name, string message) => [Create(name, message)];
}