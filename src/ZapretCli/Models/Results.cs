using System.Text.Json.Serialization;

namespace ZapretCli.Models;

public record CliError
{
    [JsonPropertyName("ok")]
    public bool Ok => false;

    [JsonPropertyName("error")]
    public string Error { get; init; } = string.Empty;

    [JsonPropertyName("details")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Details { get; init; }
}

public record ProcessInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("pid")]
    public int Pid { get; init; }
}

public record StatusState
{
    [JsonPropertyName("isRunning")]
    public bool IsRunning { get; init; }

    [JsonPropertyName("activeStrategy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ActiveStrategy { get; init; }

    [JsonPropertyName("gameFilter")]
    public string GameFilter { get; init; } = "unknown";

    [JsonPropertyName("ipsetMode")]
    public string IpsetMode { get; init; } = "unknown";

    [JsonPropertyName("processes")]
    public List<ProcessInfo> Processes { get; init; } = new();
}

public record StatusResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("state")]
    public StatusState State { get; init; } = new();
}

public record StrategyItem
{
    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; init; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;
}

public record ListStrategiesResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("strategies")]
    public List<StrategyItem> Strategies { get; init; } = new();
}

public record RunStrategyResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("started")]
    public bool Started { get; init; }

    [JsonPropertyName("strategy")]
    public string Strategy { get; init; } = string.Empty;
}

public record StopResult
{
    [JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [JsonPropertyName("stoppedProcesses")]
    public List<ProcessInfo> StoppedProcesses { get; init; } = new();
}

public record LastState
{
    public string? ActiveStrategy { get; init; }
}
