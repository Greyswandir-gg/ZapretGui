namespace ZapretGui.Models;

public class GuiSettings
{
    public string ZapretPath { get; set; } = string.Empty;
    public string GeneralMask { get; set; } = "general (*.bat)";
    public string ServiceScript { get; set; } = "service.bat";
}
