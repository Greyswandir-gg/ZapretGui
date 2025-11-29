using System.Diagnostics;
using System.IO;
using System.Text.Json;
using ZapretGui.Models;
using ZapretGui.ViewModels;

namespace ZapretGui.Services;

public class ZapretCliClient : IZapretCliClient
{
    private readonly GuiSettingsStore _settingsStore;
    private readonly DiagnosticsViewModel _diagnostics;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public ZapretCliClient(GuiSettingsStore settingsStore, DiagnosticsViewModel diagnostics)
    {
        _settingsStore = settingsStore;
        _diagnostics = diagnostics;
    }

    public async Task<CliCallResult<CliStatusResponse>> GetStatusAsync(GuiSettings? settingsOverride = null)
    {
        return await CallAsync<CliStatusResponse>("status", settingsOverride);
    }

    public async Task<CliCallResult<CliListStrategiesResponse>> ListStrategiesAsync(GuiSettings? settingsOverride = null)
    {
        return await CallAsync<CliListStrategiesResponse>("list-strategies", settingsOverride);
    }

    public async Task<CliCallResult<CliRunStrategyResponse>> RunStrategyAsync(string strategy, GuiSettings? settingsOverride = null)
    {
        return await CallAsync<CliRunStrategyResponse>($"run-strategy \"{strategy}\"", settingsOverride);
    }

    public async Task<CliCallResult<CliStopResponse>> StopAsync(GuiSettings? settingsOverride = null)
    {
        return await CallAsync<CliStopResponse>("stop", settingsOverride);
    }

    private async Task<CliCallResult<T>> CallAsync<T>(string command, GuiSettings? settingsOverride = null)
    {
        var settings = settingsOverride ?? await _settingsStore.LoadAsync();
        await _settingsStore.EnsureCliConfigAsync(settings);

        var cliPath = ResolveCliPath();
        if (cliPath is null)
        {
            const string notFound = "zapret-cli.exe не найден рядом с приложением. Переустановите или соберите проект.";
            _diagnostics.AddEntry(command, notFound, ok: false, exitCode: -1, message: notFound);
            return CliCallResult<T>.Fail("cli_not_found", null, -1, notFound);
        }

        var args = $"{command} --config \"{_settingsStore.ConfigPath}\"";
        var startInfo = new ProcessStartInfo
        {
            FileName = cliPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                const string startFailed = "Не удалось запустить zapret-cli.exe";
                _diagnostics.AddEntry(command, startFailed, ok: false, exitCode: -1, message: startFailed);
                return CliCallResult<T>.Fail("failed_to_start_cli", null, -1, startFailed);
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            var payload = string.IsNullOrWhiteSpace(output) ? error : output;
            var parsedError = TryParseError(payload);
            var friendly = MapFriendlyMessage(parsedError?.Error, parsedError?.Details);

            if (process.ExitCode != 0)
            {
                _diagnostics.AddEntry(command, payload, ok: false, exitCode: process.ExitCode, message: friendly ?? parsedError?.Error);
                return CliCallResult<T>.Fail(parsedError?.Error ?? "cli_error", payload, process.ExitCode, friendly ?? parsedError?.Details);
            }

            var result = JsonSerializer.Deserialize<T>(payload, _options);
            if (result != null)
            {
                _diagnostics.AddEntry(command, payload, ok: true, exitCode: process.ExitCode, message: friendly ?? "OK");
                return CliCallResult<T>.Success(result, payload, process.ExitCode);
            }

            const string invalidJson = "Неверный формат ответа CLI";
            _diagnostics.AddEntry(command, payload, ok: false, exitCode: process.ExitCode, message: invalidJson);
            return CliCallResult<T>.Fail("invalid_json", payload, process.ExitCode, invalidJson);
        }
        catch (Exception ex)
        {
            _diagnostics.AddEntry(command, ex.Message, ok: false, exitCode: -1, message: ex.Message);
            return CliCallResult<T>.Fail(ex.Message, null, -1, ex.Message);
        }
    }

    private CliErrorResponse? TryParseError(string payload)
    {
        try
        {
            return JsonSerializer.Deserialize<CliErrorResponse>(payload, _options);
        }
        catch
        {
            return null;
        }
    }

    private static string? MapFriendlyMessage(string? error, string? details)
    {
        return error switch
        {
            "config_not_found" => "Конфиг zapret-adapter.json не найден рядом с приложением.",
            "invalid_zapret_path" => "Некорректная папка zapret-discord-youtube, проверьте путь.",
            "strategy_not_found" => "Стратегия не найдена, выберите другую .bat.",
            "start_failed" => "Не удалось запустить BAT, подробнее в Diagnostics.",
            _ => details
        };
    }

    private static string? ResolveCliPath()
    {
        var candidate = Path.Combine(AppContext.BaseDirectory, "zapret-cli.exe");
        return File.Exists(candidate) ? candidate : null;
    }
}
