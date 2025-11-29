using ZapretCli.Models;
using ZapretCli.Services;

namespace ZapretCli.Tests;

public class StrategyRepositoryTests
{
    [Fact]
    public void ListsStrategiesUsingMask()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);

        var files = new[]
        {
            "general (ALT).bat",
            "general (ALT3).bat",
            "ignore.txt"
        };
        foreach (var file in files)
        {
            File.WriteAllText(Path.Combine(zapretDir, file), "echo test");
        }

        var repo = new StrategyRepository();
        var config = new ZapretConfig { ZapretPath = zapretDir, GeneralMask = "general (*.bat)" };

        var strategies = repo.ListStrategies(config);

        Assert.Equal(2, strategies.Count);
        Assert.Contains(strategies, s => s.DisplayName == "ALT");
        Assert.Contains(strategies, s => s.DisplayName == "ALT3");
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
