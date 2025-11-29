namespace ZapretCli.Models;

public class ZapretConfig
{
    public string ZapretPath { get; init; } = string.Empty;

    public string GeneralMask { get; init; } = "general (*.bat)";

    public string ServiceScript { get; init; } = "service.bat";
}
