using ZapretCli.Models;

namespace ZapretCli.Services;

public interface IStrategyRepository
{
    List<StrategyItem> ListStrategies(ZapretConfig config);
}
