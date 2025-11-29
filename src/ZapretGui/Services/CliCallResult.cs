namespace ZapretGui.Services;

public class CliCallResult<T>
{
    public bool Ok { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public string? Message { get; init; }
    public string? Raw { get; init; }
    public int ExitCode { get; init; } = -1;

    public static CliCallResult<T> Success(T data, string? raw = null, int exitCode = 0) =>
        new() { Ok = true, Data = data, Raw = raw, ExitCode = exitCode };

    public static CliCallResult<T> Fail(string? error, string? raw = null, int exitCode = -1, string? message = null) =>
        new() { Ok = false, Error = error, Raw = raw, ExitCode = exitCode, Message = message };
}
