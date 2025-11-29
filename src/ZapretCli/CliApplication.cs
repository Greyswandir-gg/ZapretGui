using System.CommandLine;
using ZapretCli.Configuration;
using ZapretCli.Models;
using ZapretCli.Services;

namespace ZapretCli;

public class CliApplication
{
    private readonly ConfigLoader _configLoader;
    private readonly IZapretProcessRunner _processRunner;
    private readonly IStrategyRepository _strategyRepository;
    private readonly ILastStateStore _lastStateStore;
    private readonly JsonPrinter _printer;

    public CliApplication(
        ConfigLoader configLoader,
        IZapretProcessRunner processRunner,
        IStrategyRepository strategyRepository,
        ILastStateStore lastStateStore,
        JsonPrinter printer)
    {
        _configLoader = configLoader;
        _processRunner = processRunner;
        _strategyRepository = strategyRepository;
        _lastStateStore = lastStateStore;
        _printer = printer;
    }

    public RootCommand BuildRootCommand()
    {
        var configOption = new Option<FileInfo?>(
            aliases: new[] { "--config" },
            description: "Path to zapret-adapter.json");

        var status = new Command("status", "Show zapret status");
        status.SetHandler(async (FileInfo? cfg) => await HandleStatus(cfg), configOption);

        var listStrategies = new Command("list-strategies", "List available strategies");
        listStrategies.SetHandler(async (FileInfo? cfg) => await HandleList(cfg), configOption);

        var runStrategy = new Command("run-strategy", "Run selected strategy");
        var strategyArgument = new Argument<string>("strategy", "Strategy file name");
        runStrategy.AddArgument(strategyArgument);
        runStrategy.SetHandler(async (FileInfo? cfg, string strategy) => await HandleRun(cfg, strategy),
            configOption, strategyArgument);

        var stop = new Command("stop", "Stop zapret processes");
        stop.SetHandler(async (FileInfo? cfg) => await HandleStop(cfg), configOption);

        var root = new RootCommand("zapret CLI adapter");
        root.AddGlobalOption(configOption);
        root.AddCommand(status);
        root.AddCommand(listStrategies);
        root.AddCommand(runStrategy);
        root.AddCommand(stop);
        return root;
    }

    private async Task<int> HandleStatus(FileInfo? configPath)
    {
        var configResult = _configLoader.Load(configPath?.FullName);
        if (!configResult.IsSuccess)
        {
            _printer.PrintError(configResult.Error!, configResult.Details);
            return 1;
        }

        var processes = _processRunner.GetRunningZapretProcesses();
        var lastState = await _lastStateStore.LoadAsync();

        var payload = new StatusResult
        {
            Ok = true,
            State = new StatusState
            {
                IsRunning = processes.Count > 0,
                ActiveStrategy = lastState.ActiveStrategy,
                GameFilter = "unknown",
                IpsetMode = "unknown",
                Processes = processes
                    .Select(p => new ProcessInfo { Name = p.Name, Pid = p.Pid })
                    .ToList()
            }
        };

        _printer.Print(payload);
        return 0;
    }

    private Task<int> HandleList(FileInfo? configPath)
    {
        var configResult = _configLoader.Load(configPath?.FullName);
        if (!configResult.IsSuccess)
        {
            _printer.PrintError(configResult.Error!, configResult.Details);
            return Task.FromResult(1);
        }

        var strategies = _strategyRepository.ListStrategies(configResult.Value!);
        var payload = new ListStrategiesResult
        {
            Ok = true,
            Strategies = strategies
        };

        _printer.Print(payload);
        return Task.FromResult(0);
    }

    private async Task<int> HandleRun(FileInfo? configPath, string strategyName)
    {
        var configResult = _configLoader.Load(configPath?.FullName);
        if (!configResult.IsSuccess)
        {
            _printer.PrintError(configResult.Error!, configResult.Details);
            return 1;
        }

        var config = configResult.Value!;
        var strategies = _strategyRepository.ListStrategies(config);
        var target = strategies.FirstOrDefault(s =>
            string.Equals(s.FileName, strategyName, StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            _printer.PrintError("strategy_not_found", $"Strategy '{strategyName}' not found.");
            return 1;
        }

        _processRunner.StopZapretProcesses();
        var stoppedCleanly = await EnsureStoppedAsync(TimeSpan.FromSeconds(5));
        if (!stoppedCleanly)
        {
            var stillRunning = _processRunner.GetRunningZapretProcesses();
            var details = stillRunning.Any()
                ? $"Запрет не остановился: {string.Join(',', stillRunning.Select(p => $"{p.Name}:{p.Pid}"))}"
                : "Запрет не остановился.";
            _printer.PrintError("zombie_winws", details);
            return 1;
        }

        var startResult = _processRunner.StartStrategy(target.Path, config.ZapretPath);
        if (!startResult.IsSuccess)
        {
            _printer.PrintError(startResult.Error!, startResult.Details);
            return 1;
        }

        await _lastStateStore.SaveAsync(new LastState { ActiveStrategy = target.FileName });

        var payload = new RunStrategyResult
        {
            Ok = true,
            Started = true,
            Strategy = target.FileName
        };

        _printer.Print(payload);
        return 0;
    }

    private async Task<int> HandleStop(FileInfo? configPath)
    {
        var configResult = _configLoader.Load(configPath?.FullName);
        if (!configResult.IsSuccess)
        {
            _printer.PrintError(configResult.Error!, configResult.Details);
            return 1;
        }

        var stopped = _processRunner.StopZapretProcesses();
        await _lastStateStore.SaveAsync(new LastState { ActiveStrategy = null });

        var payload = new StopResult
        {
            Ok = true,
            StoppedProcesses = stopped
                .Select(p => new ProcessInfo { Name = p.Name, Pid = p.Pid })
                .ToList()
        };

        _printer.Print(payload);
        return 0;
    }

    private async Task<bool> EnsureStoppedAsync(TimeSpan timeout)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.Elapsed < timeout)
        {
            var running = _processRunner.GetRunningZapretProcesses();
            if (running.Count == 0)
            {
                return true;
            }

            await Task.Delay(200);
        }

        return _processRunner.GetRunningZapretProcesses().Count == 0;
    }
}
