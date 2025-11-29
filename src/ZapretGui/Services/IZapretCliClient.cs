using ZapretGui.Models;

namespace ZapretGui.Services;

public interface IZapretCliClient
{
    Task<CliCallResult<CliStatusResponse>> GetStatusAsync(GuiSettings? settingsOverride = null);
    Task<CliCallResult<CliListStrategiesResponse>> ListStrategiesAsync(GuiSettings? settingsOverride = null);
    Task<CliCallResult<CliRunStrategyResponse>> RunStrategyAsync(string strategy, GuiSettings? settingsOverride = null);
    Task<CliCallResult<CliStopResponse>> StopAsync(GuiSettings? settingsOverride = null);
}
