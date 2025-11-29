using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZapretGui.Services;

namespace ZapretGui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel(GuiSettingsStore settingsStore)
    {
        Diagnostics = new DiagnosticsViewModel();
        CliClient = new ZapretCliClient(settingsStore, Diagnostics);

        Strategies = new StrategiesViewModel(CliClient);
        Resources = new ResourcesViewModel();
        Settings = new SettingsViewModel(settingsStore, CliClient);

        CurrentView = Strategies;
        SelectTabCommand = new RelayCommand<string>(SwitchTab);
    }

    public StrategiesViewModel Strategies { get; }
    public ResourcesViewModel Resources { get; }
    public DiagnosticsViewModel Diagnostics { get; }
    public SettingsViewModel Settings { get; }
    public IZapretCliClient CliClient { get; }

    [ObservableProperty]
    private object currentView;

    public IRelayCommand<string> SelectTabCommand { get; }

    public async Task InitializeAsync()
    {
        await Settings.LoadAsync();
        await Strategies.InitializeAsync();
    }

    private void SwitchTab(string? tab)
    {
        CurrentView = tab switch
        {
            "strategies" => Strategies,
            "resources" => Resources,
            "diagnostics" => Diagnostics,
            "settings" => Settings,
            _ => Strategies
        };
    }
}
