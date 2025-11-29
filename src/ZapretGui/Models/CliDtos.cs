using System.Text.Json.Serialization;

namespace ZapretGui.Models;

public class CliProcess
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pid")]
    public int Pid { get; set; }
}

public class CliStatusState
{
    [JsonPropertyName("isRunning")]
    public bool IsRunning { get; set; }

    [JsonPropertyName("activeStrategy")]
    public string? ActiveStrategy { get; set; }

    [JsonPropertyName("gameFilter")]
    public string? GameFilter { get; set; }

    [JsonPropertyName("ipsetMode")]
    public string? IpsetMode { get; set; }

    [JsonPropertyName("processes")]
    public List<CliProcess> Processes { get; set; } = new();
}

public class CliStatusResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("state")]
    public CliStatusState State { get; set; } = new();
}

public class CliStrategy
{
    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}

public class CliListStrategiesResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("strategies")]
    public List<CliStrategy> Strategies { get; set; } = new();
}

public class CliRunStrategyResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("started")]
    public bool Started { get; set; }

    [JsonPropertyName("strategy")]
    public string? Strategy { get; set; }
}

public class CliStopResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("stoppedProcesses")]
    public List<CliProcess> StoppedProcesses { get; set; } = new();
}

public class CliErrorResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }
}
