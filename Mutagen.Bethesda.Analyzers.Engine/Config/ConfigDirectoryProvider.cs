using Mutagen.Bethesda.Environments.DI;
using Noggog;
using Noggog.IO;

namespace Mutagen.Bethesda.Analyzers.Config;

public class ConfigDirectoryProvider(
    IDataDirectoryProvider dataDirectoryProvider,
    ICurrentDirectoryProvider currentDirectoryProvider)
{
    public IEnumerable<DirectoryPath> ConfigDirectories
    {
        get
        {
            yield return currentDirectoryProvider.CurrentDirectory;
            yield return dataDirectoryProvider.Path;
        }
    }
}
