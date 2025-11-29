using System.Text.Json;
using Microsoft.Extensions.Configuration;
using ZapretCli.Models;

namespace ZapretCli.Configuration;

public class ConfigLoader
{
    private readonly string _baseDirectory;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ConfigLoader(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public Result<ZapretConfig> Load(string? explicitPath)
    {
        var resolvedPath = ResolvePath(explicitPath);
        if (resolvedPath is null || !File.Exists(resolvedPath))
        {
            return Result<ZapretConfig>.Fail("config_not_found", "Configuration file was not found.");
        }

        try
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile(resolvedPath, optional: false)
                .AddEnvironmentVariables();

            var config = builder.Build().Get<ZapretConfig>() ?? new ZapretConfig();
            if (string.IsNullOrWhiteSpace(config.ZapretPath) || !Directory.Exists(config.ZapretPath))
            {
                return Result<ZapretConfig>.Fail("invalid_zapret_path", $"Invalid zapretPath: {config.ZapretPath}");
            }

            return Result<ZapretConfig>.Success(config);
        }
        catch (Exception ex)
        {
            return Result<ZapretConfig>.Fail("config_load_failed", ex.Message);
        }
    }

    private string? ResolvePath(string? explicitPath)
    {
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            return explicitPath;
        }

        var envPath = Environment.GetEnvironmentVariable("ZAPRET_ADAPTER_CONFIG");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            return envPath;
        }

        return Path.Combine(_baseDirectory, "zapret-adapter.json");
    }
}
