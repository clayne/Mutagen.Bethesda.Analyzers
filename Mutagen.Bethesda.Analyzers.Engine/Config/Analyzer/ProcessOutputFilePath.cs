namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class ProcessOutputFilePath : IConfigReaderProcessor<IAnalyzerConfig>
{
    public bool Process(IAnalyzerConfig config, IReadOnlyList<string> instructionParts, string value)
    {
        // output_file = <path>
        if (instructionParts.Count != 1) return false;

        if (instructionParts[0] is not "output_file") return false;

        config.OverrideOutputFilePath(value);

        return true;
    }
}
