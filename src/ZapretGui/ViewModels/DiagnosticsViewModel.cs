using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZapretGui.ViewModels;

public partial class DiagnosticsViewModel : ObservableObject
{
    public ObservableCollection<DiagnosticsEntry> Entries { get; } = new();

    public void AddEntry(string command, string result, bool ok, int exitCode, string? message = null)
    {
        var entry = new DiagnosticsEntry
        {
            Timestamp = DateTime.Now,
            Command = command,
            Result = result?.Trim() ?? string.Empty,
            Ok = ok,
            ExitCode = exitCode,
            Message = message
        };

        App.Current?.Dispatcher.Invoke(() => Entries.Insert(0, entry));
    }
}

public class DiagnosticsEntry
{
    public DateTime Timestamp { get; init; }
    public string Command { get; init; } = string.Empty;
    public string Result { get; init; } = string.Empty;
    public bool Ok { get; init; }
    public int ExitCode { get; init; }
    public string? Message { get; init; }
}
