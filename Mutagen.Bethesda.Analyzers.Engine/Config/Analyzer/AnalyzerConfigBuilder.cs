using System.IO.Abstractions;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class AnalyzerConfigBuilder
{
    public const string AnalyzerFileName = ".analyzerconfig";

    private readonly IFileSystem _fileSystem;
    private readonly ConfigDirectoryProvider _configDirectoryProvider;
    private readonly AnalyzerConfigReader _reader;

    public AnalyzerConfigBuilder(
        IFileSystem fileSystem,
        ConfigDirectoryProvider configDirectoryProvider,
        AnalyzerConfigReader reader)
    {
        _fileSystem = fileSystem;
        _configDirectoryProvider = configDirectoryProvider;
        _reader = reader;
    }

    public IAnalyzerConfigLookup Build()
    {
        var config = new AnalyzerConfig();

        foreach (var configDirectory in _configDirectoryProvider.ConfigDirectories)
        {
            LoadIn(Path.Combine(configDirectory.Path, AnalyzerFileName), config);
        }

        return config;
    }

    private void LoadIn(FilePath path, AnalyzerConfig config)
    {
        if (!path.CheckExists(_fileSystem)) return;

        _reader.ReadInto(path, config);
    }
}
