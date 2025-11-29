using System.Windows;
using Window = System.Windows.Window;
using ZapretGui.Services;
using ZapretGui.ViewModels;

namespace ZapretGui;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        var settingsStore = new GuiSettingsStore();
        _viewModel = new MainViewModel(settingsStore);
        DataContext = _viewModel;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeAsync();
    }
}
