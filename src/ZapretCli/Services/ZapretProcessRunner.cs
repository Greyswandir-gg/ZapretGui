using System.Diagnostics;
using ZapretCli.Models;

namespace ZapretCli.Services;

public class ZapretProcessRunner : IZapretProcessRunner
{
    private static readonly string[] ZapretProcessNames = { "winws", "winws64" };

    public IReadOnlyList<RunningProcess> GetRunningZapretProcesses()
    {
        return Process.GetProcesses()
            .Where(p => ZapretProcessNames.Any(name =>
                string.Equals(p.ProcessName, name, StringComparison.OrdinalIgnoreCase)))
            .Select(p => new RunningProcess(p.ProcessName, p.Id))
            .ToList();
    }

    public IReadOnlyList<RunningProcess> StopZapretProcesses()
    {
        var running = Process.GetProcesses()
            .Where(p => ZapretProcessNames.Any(name =>
                string.Equals(p.ProcessName, name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var stopped = new List<RunningProcess>();
        foreach (var proc in running)
        {
            try
            {
                proc.Kill(entireProcessTree: true);
                try
                {
                    proc.WaitForExit(3000);
                }
                catch
                {
                    // ignore wait errors, rely on HasExited
                }

                if (proc.HasExited)
                {
                    stopped.Add(new RunningProcess(proc.ProcessName, proc.Id));
                }
            }
            catch
            {
                // Best-effort kill; ignore failures.
            }
        }

        return stopped;
    }

    public Result<bool> StartStrategy(string scriptPath, string workingDirectory)
    {
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{scriptPath}\"",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var _ = Process.Start(info);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail("start_failed", ex.Message);
        }
    }
}
