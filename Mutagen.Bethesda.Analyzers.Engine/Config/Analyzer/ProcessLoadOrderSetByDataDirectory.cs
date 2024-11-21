using Microsoft.Extensions.Logging;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class ProcessLoadOrderSetByDataDirectory(ILogger<ProcessLoadOrderSetByDataDirectory> logger) : IConfigReaderProcessor<IAnalyzerConfig>
{
    public bool Process(IAnalyzerConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // environment.load_order.set_by_data_directory = <bool>
        if (instructionParts.Count != 3) return false;

        if (instructionParts[0] is not "environment") return false;
        if (instructionParts[1] is not "load_order") return false;
        if (instructionParts[2] is not "set_by_data_directory") return false;

        if (!bool.TryParse(value, out var setByDataDirectory))
        {
            logger.LogError("Error parsing LoadOrderSetByDataDirectory");
            return false;
        }

        config.OverrideLoadOrderSetByDataDirectory(setByDataDirectory);

        return true;
    }
}
