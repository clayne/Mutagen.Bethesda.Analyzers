using Microsoft.Extensions.Logging;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class ProcessGameRelease(ILogger<ProcessGameRelease> logger) : IConfigReaderProcessor<IAnalyzerConfig>
{
    public bool Process(IAnalyzerConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // environment.game_release = <release>
        if (instructionParts.Count != 2) return false;

        if (instructionParts[0] is not "environment") return false;
        if (instructionParts[1] is not "game_release") return false;

        if (!Enum.TryParse<GameRelease>(value, out var release))
        {
            logger.LogError("Error parsing GameRelease");
            return false;
        }

        config.OverrideGameRelease(release);

        return true;
    }
}
