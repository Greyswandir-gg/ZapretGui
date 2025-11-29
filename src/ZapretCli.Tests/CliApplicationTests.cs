using System.CommandLine;
using ZapretCli;
using ZapretCli.Configuration;
using ZapretCli.Models;
using ZapretCli.Services;

namespace ZapretCli.Tests;

public class CliApplicationTests
{
    [Fact]
    public async Task RunStrategy_InvokesRunnerAndPersistsState()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var strategyPath = Path.Combine(zapretDir, "general (ALT3).bat");
        File.WriteAllText(strategyPath, "echo test");

        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\",\"generalMask\":\"*.bat\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner();
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        var output = await InvokeAndCaptureAsync(root, new[] { "run-strategy", "--config", configPath, "general (ALT3).bat" });

        Assert.Equal(strategyPath, fakeRunner.StartedPath);
        Assert.Equal("general (ALT3).bat", stateStore.LastState?.ActiveStrategy);
        Assert.Contains("\"ok\":true", output);
        Assert.Contains("\"strategy\":\"general (ALT3).bat\"", output);
    }

    [Fact]
    public async Task Status_ReturnsRunningStateFromProcessList()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner();
        fakeRunner.Running.Add(new RunningProcess("winws", 1234));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore(new LastState { ActiveStrategy = "general (ALT).bat" });
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        var output = await InvokeAndCaptureAsync(root, new[] { "status", "--config", configPath });

        Assert.Contains("\"isRunning\":true", output);
        Assert.Contains("\"activeStrategy\":\"general (ALT).bat\"", output);
        Assert.Contains("\"pid\":1234", output);
    }

    [Fact]
    public async Task StopThenStatus_ReturnsNotRunning()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner();
        fakeRunner.Running.Add(new RunningProcess("winws", 23580));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore(new LastState { ActiveStrategy = "general (ALT2).bat" });
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        await InvokeAndCaptureAsync(root, new[] { "stop", "--config", configPath });
        var status = await InvokeAndCaptureAsync(root, new[] { "status", "--config", configPath });

        Assert.Contains("\"isRunning\":false", status);
        Assert.DoesNotContain("\"pid\":23580", status);
    }

    [Fact]
    public async Task Status_NoProcesses_ReturnsNotRunning()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner();
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        var status = await InvokeAndCaptureAsync(root, new[] { "status", "--config", configPath });

        Assert.Contains("\"isRunning\":false", status);
        Assert.DoesNotContain("\"pid\":", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RunStrategy_KillsPreviousProcess()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var strategyPath = Path.Combine(zapretDir, "general (ALT2).bat");
        File.WriteAllText(strategyPath, "echo alt2");
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\",\"generalMask\":\"*.bat\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner { StartAddsProcess = true };
        fakeRunner.Running.Add(new RunningProcess("winws", 1111));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        await InvokeAndCaptureAsync(root, new[] { "run-strategy", "--config", configPath, "general (ALT2).bat" });

        Assert.DoesNotContain(fakeRunner.Running, p => p.Pid == 1111);
        Assert.True(fakeRunner.Running.Any(), "New process should start");
    }

    [Fact]
    public async Task RunStrategy_FailsIfOldProcessDoesNotExit()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var strategyPath = Path.Combine(zapretDir, "general (ALT3).bat");
        File.WriteAllText(strategyPath, "echo alt3");
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\",\"generalMask\":\"*.bat\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner { StopClears = false }; // simulate stuck process
        fakeRunner.Running.Add(new RunningProcess("winws", 2222));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        var output = await InvokeAndCaptureAsync(root, new[] { "run-strategy", "--config", configPath, "general (ALT3).bat" });

        Assert.Contains("zombie_winws", output);
        Assert.Null(fakeRunner.StartedPath);
    }

    [Fact]
    public async Task SwitchStrategy_StopsOldThenStartsNew()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var strategyAlt2 = Path.Combine(zapretDir, "general (ALT2).bat");
        var strategyAlt10 = Path.Combine(zapretDir, "general (ALT10).bat");
        File.WriteAllText(strategyAlt2, "echo alt2");
        File.WriteAllText(strategyAlt10, "echo alt10");
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\",\"generalMask\":\"*.bat\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner { StartAddsProcess = true };
        fakeRunner.Running.Add(new RunningProcess("winws", 3333));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        await InvokeAndCaptureAsync(root, new[] { "run-strategy", "--config", configPath, "general (ALT10).bat" });

        Assert.True(fakeRunner.Running.All(p => p.Pid != 3333));
        Assert.Equal(strategyAlt10, fakeRunner.StartedPath);
    }

    [Fact]
    public async Task StatusAfterSwitch_ShowsNewPid()
    {
        using var ctx = new TempContext();
        var zapretDir = Path.Combine(ctx.WorkDir, "zapret");
        Directory.CreateDirectory(zapretDir);
        var strategyAlt2 = Path.Combine(zapretDir, "general (ALT2).bat");
        var strategyAlt10 = Path.Combine(zapretDir, "general (ALT10).bat");
        File.WriteAllText(strategyAlt2, "echo alt2");
        File.WriteAllText(strategyAlt10, "echo alt10");
        var configPath = Path.Combine(ctx.WorkDir, "zapret-adapter.json");
        File.WriteAllText(configPath, $"{{\"zapretPath\":\"{zapretDir.Replace("\\", "\\\\")}\",\"generalMask\":\"*.bat\"}}");

        var configLoader = new ConfigLoader(ctx.WorkDir);
        var fakeRunner = new FakeRunner { StartAddsProcess = true };
        fakeRunner.Running.Add(new RunningProcess("winws", 4444));
        var repository = new StrategyRepository();
        var stateStore = new InMemoryLastStateStore();
        var printer = new JsonPrinter();
        var app = new CliApplication(configLoader, fakeRunner, repository, stateStore, printer);

        var root = app.BuildRootCommand();
        await InvokeAndCaptureAsync(root, new[] { "run-strategy", "--config", configPath, "general (ALT10).bat" });
        var status = await InvokeAndCaptureAsync(root, new[] { "status", "--config", configPath });

        Assert.DoesNotContain("\"pid\":4444", status);
        Assert.Contains("\"pid\":9000", status); // first started PID from FakeRunner
    }

    private static async Task<string> InvokeAndCaptureAsync(RootCommand root, string[] args)
    {
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await root.InvokeAsync(args);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        return writer.ToString();
    }

    private sealed class FakeRunner : IZapretProcessRunner
    {
        public List<RunningProcess> Running { get; } = new();
        public string? StartedPath { get; private set; }
        public string? StartedWorkingDirectory { get; private set; }
        public bool StopClears { get; set; } = true;
        public bool StartAddsProcess { get; set; }
        public int NextPid { get; set; } = 9000;

        public IReadOnlyList<RunningProcess> GetRunningZapretProcesses() => Running;

        public IReadOnlyList<RunningProcess> StopZapretProcesses()
        {
            var snapshot = Running.ToList();
            if (StopClears)
            {
                Running.Clear();
            }

            return snapshot;
        }

        public Result<bool> StartStrategy(string scriptPath, string workingDirectory)
        {
            StartedPath = scriptPath;
            StartedWorkingDirectory = workingDirectory;
            if (StartAddsProcess)
            {
                Running.Add(new RunningProcess("winws", NextPid++));
            }

            return Result<bool>.Success(true);
        }
    }

    private sealed class InMemoryLastStateStore : ILastStateStore
    {
        public LastState? LastState { get; private set; }

        public InMemoryLastStateStore(LastState? initial = null)
        {
            LastState = initial;
        }

        public Task<LastState> LoadAsync() => Task.FromResult(LastState ?? new LastState());

        public Task SaveAsync(LastState state)
        {
            LastState = state;
            return Task.CompletedTask;
        }
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
