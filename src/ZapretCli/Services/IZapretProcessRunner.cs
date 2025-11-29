using ZapretCli.Models;

namespace ZapretCli.Services;

public interface IZapretProcessRunner
{
    IReadOnlyList<RunningProcess> GetRunningZapretProcesses();

    IReadOnlyList<RunningProcess> StopZapretProcesses();

    Result<bool> StartStrategy(string scriptPath, string workingDirectory);
}
