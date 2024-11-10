using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Topic;

public class TopicConfigReader : IConfigReader<ITopicConfig>
{
    private const string DiagnosticGroup = "diagnostic";

    private readonly ConfigReader<ITopicConfig> _reader;
    private readonly ILogger<TopicConfigReader> _logger;

    public TopicConfigReader(
        ConfigReader<ITopicConfig> reader,
        ILogger<TopicConfigReader> logger)
    {
        _logger = logger;
        _reader = reader
            .Register(ProcessSeverity);
    }

    public void ReadInto(FilePath path, ITopicConfig config) => _reader.ReadInto(path, config);
    public void ReadInto(ReadOnlySpan<char> line, ITopicConfig config) => _reader.ReadInto(line, config);

    private bool ProcessSeverity(ITopicConfig config, IReadOnlyList<string> instructions, string value)
    {
        // diagnostic.<TopicId>.severity = <Severity>
        if (instructions.Count != 3) return false;

        if (instructions[0] is not DiagnosticGroup) return false;

        TopicId id;
        try
        {
            id = TopicId.Parse(instructions[1]);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error parsing TopicId");
            return false;
        }

        if (instructions[2] is not "severity") return false;

        if (!Enum.TryParse<Severity>(value, out var severity))
        {
            _logger.LogError("Error parsing Severity");
            return false;
        }

        config.Override(id, severity);
        return true;
    }
}
