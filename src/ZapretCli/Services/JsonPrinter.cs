using System.Text.Json;
using ZapretCli.Models;

namespace ZapretCli.Services;

public class JsonPrinter
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public void Print<T>(T payload)
    {
        var json = JsonSerializer.Serialize(payload, _options);
        Console.Out.WriteLine(json);
    }

    public void PrintError(string error, string? details = null)
    {
        Print(new CliError { Error = error, Details = details });
    }
}
