using System.IO.Abstractions;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Topic;

public class TopicConfigBuilder
{
    public const string TopicFileName = ".topicconfig";

    private readonly IFileSystem _fileSystem;
    private readonly ConfigDirectoryProvider _configDirectoryProvider;
    private readonly TopicConfigReader _reader;

    public TopicConfigBuilder(
        IFileSystem fileSystem,
        ConfigDirectoryProvider configDirectoryProvider,
        TopicConfigReader reader)
    {
        _fileSystem = fileSystem;
        _configDirectoryProvider = configDirectoryProvider;
        _reader = reader;
    }

    public ITopicConfig Build()
    {
        var config = new TopicConfig();

        foreach (var configDirectory in _configDirectoryProvider.ConfigDirectories)
        {
            LoadIn(Path.Combine(configDirectory.Path, TopicFileName), config);
        }

        return config;
    }

    private void LoadIn(FilePath path, TopicConfig config)
    {
        if (!path.CheckExists(_fileSystem)) return;

        _reader.ReadInto(path, config);
    }
}
