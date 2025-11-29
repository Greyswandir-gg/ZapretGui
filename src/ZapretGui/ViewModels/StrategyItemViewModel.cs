using CommunityToolkit.Mvvm.ComponentModel;

namespace ZapretGui.ViewModels;

public partial class StrategyItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string path = string.Empty;
}
