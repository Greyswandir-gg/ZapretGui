using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGui.Models;
using ZapretGui.Services;

namespace ZapretGui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly GuiSettingsStore _settingsStore;
    private readonly IZapretCliClient _cliClient;

    [ObservableProperty] private string zapretPath = string.Empty;
    [ObservableProperty] private string generalMask = "general (*.bat)";
    [ObservableProperty] private string serviceScript = "service.bat";
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool isBusy;

    public SettingsViewModel(GuiSettingsStore settingsStore, IZapretCliClient cliClient)
    {
        _settingsStore = settingsStore;
        _cliClient = cliClient;

        BrowseZapretPathCommand = new RelayCommand(SelectFolder);
        SaveSettingsCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
        TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, () => !IsBusy);
    }

    public IRelayCommand BrowseZapretPathCommand { get; }
    public IAsyncRelayCommand SaveSettingsCommand { get; }
    public IAsyncRelayCommand TestConnectionCommand { get; }

    public async Task LoadAsync()
    {
        var settings = await _settingsStore.LoadAsync();
        ZapretPath = settings.ZapretPath;
        GeneralMask = settings.GeneralMask;
        ServiceScript = settings.ServiceScript;
    }

    partial void OnIsBusyChanged(bool value)
    {
        SaveSettingsCommand.NotifyCanExecuteChanged();
        TestConnectionCommand.NotifyCanExecuteChanged();
    }

    private void SelectFolder()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = "Choose zapret-discord-youtube folder";
        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ZapretPath = dialog.SelectedPath;
        }
    }

    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var settings = BuildSettings();
            if (!_settingsStore.IsValidZapretPath(settings.ZapretPath, settings.GeneralMask, settings.ServiceScript))
            {
                StatusMessage = "Некорректная папка zapret-discord-youtube (нет general*.bat или service.bat).";
                return;
            }

            await _settingsStore.SaveAsync(settings);
            await _settingsStore.EnsureCliConfigAsync(settings);
            StatusMessage = "Settings saved.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        if (IsBusy) return;
        var settings = BuildSettings();
        if (!_settingsStore.IsValidZapretPath(settings.ZapretPath, settings.GeneralMask, settings.ServiceScript))
        {
            StatusMessage = "Некорректная папка zapret-discord-youtube, проверьте путь.";
            return;
        }

        StatusMessage = "Testing...";
        IsBusy = true;
        try
        {
            await _settingsStore.SaveAsync(settings);
            await _settingsStore.EnsureCliConfigAsync(settings);
            var result = await _cliClient.GetStatusAsync(settings);
            StatusMessage = result.Ok
                ? "CLI reachable."
                : result.Message ?? result.Error ?? "CLI error.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private GuiSettings BuildSettings() => new()
    {
        ZapretPath = ZapretPath,
        GeneralMask = GeneralMask,
        ServiceScript = ServiceScript
    };
}
