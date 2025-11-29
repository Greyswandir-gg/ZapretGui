using System.Text.Json;
using ZapretCli.Models;

namespace ZapretCli.Services;

public interface ILastStateStore
{
    Task<LastState> LoadAsync();
    Task SaveAsync(LastState state);
}

public class FileLastStateStore : ILastStateStore
{
    private readonly string _primaryPath;
    private readonly string _fallbackPath;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public FileLastStateStore(string baseDirectory)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _primaryPath = Path.Combine(appData, "zapret-gui", "last-state.json");
        _fallbackPath = Path.Combine(baseDirectory, "last-state.json");
    }

    public async Task<LastState> LoadAsync()
    {
        var path = File.Exists(_primaryPath) ? _primaryPath : _fallbackPath;
        if (!File.Exists(path))
        {
            return new LastState();
        }

        try
        {
            var text = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<LastState>(text, _options) ?? new LastState();
        }
        catch
        {
            return new LastState();
        }
    }

    public async Task SaveAsync(LastState state)
    {
        try
        {
            var path = _primaryPath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(state, _options);
            await File.WriteAllTextAsync(path, json);
        }
        catch
        {
            // Intentionally swallow errors to avoid failing the CLI for persistence issues.
        }
    }
}
