using ZapretCli.Configuration;

namespace ZapretCli.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void UsesExplicitPathWhenProvided()
    {
        using var ctx = new TempContext();
        var configPath = Path.Combine(ctx.WorkDir, "explicit.json");
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\"}}");

        var loader = new ConfigLoader(ctx.WorkDir);
        var result = loader.Load(configPath);

        Assert.True(result.IsSuccess);
        Assert.Equal(zapretDir, result.Value!.ZapretPath);
    }

    [Fact]
    public void UsesEnvironmentVariableWhenNoExplicitPath()
    {
        using var ctx = new TempContext();
        var configPath = Path.Combine(ctx.WorkDir, "from-env.json");
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\"}}");

        Environment.SetEnvironmentVariable("ZAPRET_ADAPTER_CONFIG", configPath);
        try
        {
            var loader = new ConfigLoader(ctx.WorkDir);
            var result = loader.Load(null);
            Assert.True(result.IsSuccess);
            Assert.Equal(zapretDir, result.Value!.ZapretPath);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ZAPRET_ADAPTER_CONFIG", null);
        }
    }

    [Fact]
    public void InvalidPathReturnsError()
    {
        using var ctx = new TempContext();
        var configPath = Path.Combine(ctx.WorkDir, "bad.json");
        File.WriteAllText(configPath, "{\"zapretPath\":\"C:\\\\missing\"}");

        var loader = new ConfigLoader(ctx.WorkDir);
        var result = loader.Load(configPath);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_zapret_path", result.Error);
    }

    private sealed class TempContext : IDisposable
    {
        public string WorkDir { get; } = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public TempContext()
        {
            Directory.CreateDirectory(WorkDir);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(WorkDir, recursive: true);
            }
            catch
            {
                // best-effort cleanup
            }
        }
    }
}
