using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGui.Services;

namespace ZapretGui.ViewModels;

public partial class StrategiesViewModel : ObservableObject
{
    private readonly IZapretCliClient _cli;

    public StrategiesViewModel(IZapretCliClient cli)
    {
        _cli = cli;
        Strategies = new ObservableCollection<StrategyItemViewModel>();
        RefreshStatusCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        RunSelectedCommand = new AsyncRelayCommand(RunSelectedAsync, CanRun);
        StopCommand = new AsyncRelayCommand(StopAsync, CanStop);
    }

    public ObservableCollection<StrategyItemViewModel> Strategies { get; }

    [ObservableProperty]
    private StrategyItemViewModel? selectedStrategy;

    partial void OnSelectedStrategyChanged(StrategyItemViewModel? value)
    {
        UpdateCommandStates();
    }

    [ObservableProperty]
    private bool isRunning;

    partial void OnIsRunningChanged(bool value)
    {
        UpdateCommandStates();
    }

    [ObservableProperty]
    private bool isBusy;

    partial void OnIsBusyChanged(bool value)
    {
        UpdateCommandStates();
    }

    [ObservableProperty]
    private bool cliAvailable = true;

    partial void OnCliAvailableChanged(bool value)
    {
        UpdateCommandStates();
    }

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand RefreshStatusCommand { get; }
    public IAsyncRelayCommand RunSelectedCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }

    public async Task InitializeAsync()
    {
        await RefreshAsync();
    }

    private bool CanRun() => SelectedStrategy != null && !IsBusy && CliAvailable;
    private bool CanStop() => !IsBusy && IsRunning && CliAvailable;

    private void UpdateCommandStates()
    {
        RunSelectedCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        RefreshStatusCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(DebugState));
    }

    private async Task RefreshAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = string.Empty;

        try
        {
            await LoadStrategiesAsync();
            await LoadStatusAsync();
        }
        finally
        {
            IsBusy = false;
            UpdateCommandStates();
        }
    }

    private async Task LoadStrategiesAsync()
    {
        var response = await _cli.ListStrategiesAsync();
        if (!response.Ok || response.Data == null)
        {
            CliAvailable = false;
            StatusMessage = response.Message ?? response.Error ?? "Failed to load strategies.";
            return;
        }

        CliAvailable = true;
        Strategies.Clear();
        foreach (var item in response.Data.Strategies)
        {
            Strategies.Add(new StrategyItemViewModel
            {
                DisplayName = item.DisplayName,
                FileName = item.FileName,
                Path = item.Path
            });
        }

        if (SelectedStrategy == null && Strategies.Any())
        {
            SelectedStrategy = Strategies.First();
        }
    }

    private async Task LoadStatusAsync()
    {
        var response = await _cli.GetStatusAsync();
        if (!response.Ok || response.Data == null)
        {
            CliAvailable = false;
            StatusMessage = response.Message ?? response.Error ?? "Failed to load status.";
            return;
        }

        CliAvailable = true;
        IsRunning = response.Data.State.IsRunning;
        if (!string.IsNullOrWhiteSpace(response.Data.State.ActiveStrategy))
        {
            SelectedStrategy = Strategies.FirstOrDefault(
                s => string.Equals(s.FileName, response.Data.State.ActiveStrategy, StringComparison.OrdinalIgnoreCase))
                ?? SelectedStrategy;
        }
    }

    private async Task RunSelectedAsync()
    {
        if (SelectedStrategy == null) return;
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var response = await _cli.RunStrategyAsync(SelectedStrategy.FileName);
            if (!response.Ok)
            {
                CliAvailable = false;
                StatusMessage = response.Message ?? response.Error ?? "Failed to start strategy.";
                return;
            }

            CliAvailable = true;
            IsRunning = true;
            StatusMessage = $"Started {SelectedStrategy.DisplayName}";
        }
        finally
        {
            IsBusy = false;
            UpdateCommandStates();
        }
    }

    private async Task StopAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var response = await _cli.StopAsync();
            if (!response.Ok)
            {
                CliAvailable = false;
                StatusMessage = response.Message ?? response.Error ?? "Failed to stop.";
                return;
            }

            CliAvailable = true;
            IsRunning = false;
            StatusMessage = "Stopped zapret processes.";
        }
        finally
        {
            IsBusy = false;
            UpdateCommandStates();
        }
    }

    public string DebugState =>
        $"busy={IsBusy} run={IsRunning} sel={SelectedStrategy != null} cli={CliAvailable}";
}
