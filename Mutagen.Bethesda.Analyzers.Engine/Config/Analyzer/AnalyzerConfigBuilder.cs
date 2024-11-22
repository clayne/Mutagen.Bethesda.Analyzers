using System.IO.Abstractions;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class AnalyzerConfigBuilder(
    IFileSystem fileSystem,
    ConfigDirectoryProvider configDirectoryProvider,
    ConfigReader<IAnalyzerConfig> reader)
{
    public const string AnalyzerFileName = ".analyzerconfig";

    public IAnalyzerConfigLookup Build()
    {
        var config = new AnalyzerConfig();

        foreach (var configDirectory in configDirectoryProvider.ConfigDirectories)
        {
            LoadIn(Path.Combine(configDirectory.Path, AnalyzerFileName), config);
        }

        return config;
    }

    private void LoadIn(FilePath path, AnalyzerConfig config)
    {
        if (!path.CheckExists(fileSystem)) return;

        reader.ReadInto(path, config);
    }
}
