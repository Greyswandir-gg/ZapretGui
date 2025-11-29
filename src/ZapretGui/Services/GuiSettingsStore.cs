using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ZapretGui.Models;

namespace ZapretGui.Services;

public class GuiSettingsStore
{
    private readonly string _settingsPath;
    private readonly string _configPath;
    private readonly string? _exampleConfigPath;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public GuiSettingsStore()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var baseDir = Path.Combine(appData, "zapret-gui");
        Directory.CreateDirectory(baseDir);
        _settingsPath = Path.Combine(baseDir, "gui-settings.json");
        _configPath = Path.Combine(baseDir, "zapret-adapter.json");
        _exampleConfigPath = FindExampleConfig();
    }

    public string ConfigPath => _configPath;

    public async Task<GuiSettings> LoadAsync()
    {
        var settings = new GuiSettings();

        var adapterConfig = await LoadAdapterConfigAsync();
        if (adapterConfig is not null)
        {
            settings.ZapretPath = adapterConfig.ZapretPath;
            settings.GeneralMask = adapterConfig.GeneralMask;
            settings.ServiceScript = adapterConfig.ServiceScript;
        }

        if (File.Exists(_settingsPath))
        {
            try
            {
                var text = await File.ReadAllTextAsync(_settingsPath);
                var saved = JsonSerializer.Deserialize<GuiSettings>(text, _options);
                if (saved is not null)
                {
                    settings.ZapretPath = string.IsNullOrWhiteSpace(saved.ZapretPath) ? settings.ZapretPath : saved.ZapretPath;
                    settings.GeneralMask = string.IsNullOrWhiteSpace(saved.GeneralMask) ? settings.GeneralMask : saved.GeneralMask;
                    settings.ServiceScript = string.IsNullOrWhiteSpace(saved.ServiceScript) ? settings.ServiceScript : saved.ServiceScript;
                }
            }
            catch
            {
                // ignore corrupted GUI settings, fallback to adapter/defaults
            }
        }

        return settings;
    }

    public async Task SaveAsync(GuiSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _options);
        await File.WriteAllTextAsync(_settingsPath, json);
    }

    public async Task EnsureCliConfigAsync(GuiSettings settings)
    {
        var adapter = new
        {
            zapretPath = settings.ZapretPath,
            generalMask = settings.GeneralMask,
            serviceScript = settings.ServiceScript
        };

        var json = JsonSerializer.Serialize(adapter, _options);
        await File.WriteAllTextAsync(_configPath, json);
    }

    public bool IsValidZapretPath(string? path, string generalMask, string serviceScript)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            return false;
        }

        try
        {
            var normalizedMask = NormalizeMask(string.IsNullOrWhiteSpace(generalMask) ? "*.bat" : generalMask);
            var hasMask = Directory.EnumerateFiles(path, normalizedMask, SearchOption.TopDirectoryOnly).Any();
            var hasService = File.Exists(Path.Combine(path, serviceScript));
            return hasMask || hasService;
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeMask(string mask)
    {
        // Align with CLI logic: make "(*.bat)" patterns match "(ALT3).bat" filenames.
        return mask.Replace("(*.bat)", "(*).bat", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<AdapterConfig?> LoadAdapterConfigAsync()
    {
        if (File.Exists(_configPath))
        {
            try
            {
                var text = await File.ReadAllTextAsync(_configPath);
                var cfg = JsonSerializer.Deserialize<AdapterConfig>(text, _options);
                if (cfg is not null)
                {
                    return cfg;
                }
            }
            catch
            {
                // fall back
            }
        }

        if (!string.IsNullOrWhiteSpace(_exampleConfigPath) && File.Exists(_exampleConfigPath))
        {
            try
            {
                var text = await File.ReadAllTextAsync(_exampleConfigPath);
                return JsonSerializer.Deserialize<AdapterConfig>(text, _options);
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }

    private static string? FindExampleConfig()
    {
        var baseDir = AppContext.BaseDirectory;
        var current = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && current != null; i++)
        {
            var candidate = Path.Combine(current.FullName, "config", "zapret-adapter.example.json");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        return null;
    }

    private record AdapterConfig
    {
        public string ZapretPath { get; init; } = string.Empty;
        public string GeneralMask { get; init; } = "general (*.bat)";
        public string ServiceScript { get; init; } = "service.bat";
    }
}
