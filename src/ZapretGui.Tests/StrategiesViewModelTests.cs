using System;
using CommunityToolkit.Mvvm.Input;
using ZapretGui.Models;
using ZapretGui.Services;
using ZapretGui.ViewModels;

namespace ZapretGui.Tests;

public class StrategiesViewModelTests
{
    [Fact]
    public async Task RunStrategy_SetsBusyAndClearsAfterSuccess()
    {
        var fake = new FakeCli
        {
            RunResult = CliCallResult<CliRunStrategyResponse>.Success(new CliRunStrategyResponse { Ok = true }),
            RunDelay = TimeSpan.FromMilliseconds(10)
        };
        var vm = new StrategiesViewModel(fake);
        vm.SelectedStrategy = new StrategyItemViewModel { DisplayName = "ALT", FileName = "general (ALT).bat" };
        vm.RunSelectedCommand.NotifyCanExecuteChanged();

        var cmd = Assert.IsType<AsyncRelayCommand>(vm.RunSelectedCommand);
        Assert.NotNull(vm.SelectedStrategy);
        Assert.False(vm.IsBusy);
        Assert.False(cmd.IsRunning);
        var canBefore = vm.RunSelectedCommand.CanExecute(null);
        Console.WriteLine($"[success case] can={canBefore} selected={vm.SelectedStrategy != null} busy={vm.IsBusy} running={cmd.IsRunning}");
        Assert.True(canBefore,
            $"Selected:{vm.SelectedStrategy != null}, IsBusy:{vm.IsBusy}, IsRunning:{cmd.IsRunning}");

        var task = vm.RunSelectedCommand.ExecuteAsync(null);
        await Task.Delay(5);
        Assert.True(vm.IsBusy);
        await task;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsRunning);
        Assert.True(vm.RunSelectedCommand.CanExecute(null)); // still runnable after completion
        Assert.True(vm.StopCommand.CanExecute(null));
    }

    [Fact]
    public async Task RunStrategy_ErrorClearsBusyAndKeepsCommandsEnabled()
    {
        var fake = new FakeCli
        {
            RunResult = CliCallResult<CliRunStrategyResponse>.Fail("cli_error", message: "boom"),
            RunDelay = TimeSpan.FromMilliseconds(10)
        };
        var vm = new StrategiesViewModel(fake);
        vm.SelectedStrategy = new StrategyItemViewModel { DisplayName = "ALT", FileName = "general (ALT).bat" };
        vm.RunSelectedCommand.NotifyCanExecuteChanged();

        var cmd = Assert.IsType<AsyncRelayCommand>(vm.RunSelectedCommand);
        Assert.NotNull(vm.SelectedStrategy);
        Assert.False(vm.IsBusy);
        var canBefore = vm.RunSelectedCommand.CanExecute(null);
        Console.WriteLine($"[error case] can={canBefore} selected={vm.SelectedStrategy != null} busy={vm.IsBusy} running={cmd.IsRunning}");
        Assert.True(canBefore,
            $"Selected:{vm.SelectedStrategy != null}, IsBusy:{vm.IsBusy}, IsRunning:{cmd.IsRunning}");

        var task = vm.RunSelectedCommand.ExecuteAsync(null);
        await Task.Delay(5);
        Assert.True(vm.IsBusy);
        await task;

        Assert.False(vm.IsBusy);
        Assert.False(vm.IsRunning);
        Assert.Contains("boom", vm.StatusMessage, StringComparison.OrdinalIgnoreCase);
        Assert.False(vm.CliAvailable);
        Assert.False(vm.RunSelectedCommand.CanExecute(null));
        Assert.False(vm.StopCommand.CanExecute(null));
    }

    [Fact]
    public async Task RunThenStop_ReenablesRunAndDisablesStop()
    {
        var fake = new FakeCli
        {
            RunResult = CliCallResult<CliRunStrategyResponse>.Success(new CliRunStrategyResponse { Ok = true }),
            RunDelay = TimeSpan.FromMilliseconds(10),
            StopDelay = TimeSpan.FromMilliseconds(10)
        };

        var vm = new StrategiesViewModel(fake);
        vm.SelectedStrategy = new StrategyItemViewModel { DisplayName = "ALT2", FileName = "general (ALT2).bat" };

        Assert.True(vm.RunSelectedCommand.CanExecute(null));

        await vm.RunSelectedCommand.ExecuteAsync(null);
        Assert.True(vm.IsRunning);
        Assert.True(vm.StopCommand.CanExecute(null));

        await vm.StopCommand.ExecuteAsync(null);
        Assert.False(vm.IsRunning);
        Assert.True(vm.RunSelectedCommand.CanExecute(null));
        Assert.False(vm.StopCommand.CanExecute(null));
    }

    private sealed class FakeCli : IZapretCliClient
    {
        public CliCallResult<CliRunStrategyResponse>? RunResult { get; set; }
        public TimeSpan? RunDelay { get; set; }
        public TimeSpan? StopDelay { get; set; }

        public Task<CliCallResult<CliStatusResponse>> GetStatusAsync(GuiSettings? settingsOverride = null) =>
            Task.FromResult(CliCallResult<CliStatusResponse>.Success(new CliStatusResponse { Ok = true }));

        public Task<CliCallResult<CliListStrategiesResponse>> ListStrategiesAsync(GuiSettings? settingsOverride = null) =>
            Task.FromResult(CliCallResult<CliListStrategiesResponse>.Success(new CliListStrategiesResponse
            {
                Ok = true,
                Strategies = new List<CliStrategy>()
            }));

        public Task<CliCallResult<CliRunStrategyResponse>> RunStrategyAsync(string strategy, GuiSettings? settingsOverride = null) =>
            RunInternalAsync(RunResult ?? CliCallResult<CliRunStrategyResponse>.Fail("not_set"));

        public Task<CliCallResult<CliStopResponse>> StopAsync(GuiSettings? settingsOverride = null) =>
            RunStopAsync();

        private async Task<CliCallResult<T>> RunInternalAsync<T>(CliCallResult<T> result)
        {
            if (RunDelay.HasValue)
            {
                await Task.Delay(RunDelay.Value);
            }

            return result;
        }

        private async Task<CliCallResult<CliStopResponse>> RunStopAsync()
        {
            if (StopDelay.HasValue)
            {
                await Task.Delay(StopDelay.Value);
            }

            return CliCallResult<CliStopResponse>.Success(new CliStopResponse { Ok = true });
        }
    }
}
