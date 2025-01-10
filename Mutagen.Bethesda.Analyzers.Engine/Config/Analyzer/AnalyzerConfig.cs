using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public interface IAnalyzerConfigLookup
{
    DirectoryPath? DataDirectoryPath { get; }
    IEnumerable<ModKey>? LoadOrderSetToMods { get; }
    FilePath? OutputFilePath { get; }
}

public interface IAnalyzerConfig : IAnalyzerConfigLookup
{
    void OverrideDataDirectory(DirectoryPath path);
    void OverrideOutputFilePath(FilePath filePath);
    void OverrideLoadOrderSetToMods(IEnumerable<ModKey> mods);
}

public class AnalyzerConfig : IAnalyzerConfig
{
    public DirectoryPath? DataDirectoryPath { get; private set; }
    public FilePath? OutputFilePath { get; private set; }
    public IEnumerable<ModKey>? LoadOrderSetToMods { get; private set; }

    public void OverrideDataDirectory(DirectoryPath path) => DataDirectoryPath = path;
    public void OverrideOutputFilePath(FilePath filePath) => OutputFilePath = filePath;
    public void OverrideLoadOrderSetToMods(IEnumerable<ModKey> mods) => LoadOrderSetToMods = mods;
}
