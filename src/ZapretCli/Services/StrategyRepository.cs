using System.Text.RegularExpressions;
using ZapretCli.Models;

namespace ZapretCli.Services;

public class StrategyRepository : IStrategyRepository
{
    public List<StrategyItem> ListStrategies(ZapretConfig config)
    {
        var items = new List<StrategyItem>();
        if (!Directory.Exists(config.ZapretPath))
        {
            return items;
        }

        var mask = NormalizeMask(string.IsNullOrWhiteSpace(config.GeneralMask) ? "*.bat" : config.GeneralMask);
        var files = Directory.GetFiles(config.ZapretPath, mask, SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            items.Add(new StrategyItem
            {
                FileName = fileName,
                DisplayName = ExtractDisplayName(fileName),
                Path = file
            });
        }

        return items.OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string NormalizeMask(string mask)
    {
        // Convert "(*.bat)" style patterns into "(*)\.bat" equivalent for GetFiles.
        return mask.Replace("(*.bat)", "(*).bat", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractDisplayName(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var match = Regex.Match(nameWithoutExt, @"\((?<name>.+?)\)$");
        return match.Success ? match.Groups["name"].Value : nameWithoutExt;
    }
}
