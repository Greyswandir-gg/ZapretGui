namespace ZapretCli.Models;

public record Result<T>
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public string? Details { get; init; }
    public T? Value { get; init; }

    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static Result<T> Fail(string error, string? details = null) =>
        new() { IsSuccess = false, Error = error, Details = details };
}
