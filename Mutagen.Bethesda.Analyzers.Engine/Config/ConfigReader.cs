using Microsoft.Extensions.Logging;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config;

public class ConfigReader<TConfig> : IConfigReader<TConfig> where TConfig : class
{
    public const string SettingEqualString = " = ";

    private readonly ILogger<ConfigReader<TConfig>> _logger;
    private readonly List<Func<TConfig, IReadOnlyList<string>, string, bool>> _processors = [];

    public ConfigReader(ILogger<ConfigReader<TConfig>> logger)
    {
        _logger = logger;
    }

    public ConfigReader<TConfig> Register(Func<TConfig, IReadOnlyList<string>, string, bool> process)
    {
        var configReader = new ConfigReader<TConfig>(_logger);
        configReader._processors.AddRange(_processors);
        configReader._processors.Add(process);
        return configReader;
    }

    public void ReadInto(FilePath path, TConfig config)
    {
        foreach (var line in File.ReadLines(path))
        {
            var span = line.AsSpan();
            ReadInto(span, config);
        }
    }

    public void ReadInto(ReadOnlySpan<char> line, TConfig config)
    {
        // Remove comments
        var index = line.IndexOf("#");
        if (index != -1)
        {
            line = line.Slice(0, index);
        }

        // Remove empty lines
        if (line.IsWhiteSpace()) return;

        // Separate instruction and its assigned value
        Span<Range> ranges = stackalloc Range[2];
        try
        {
            var split = line.Split(ranges, SettingEqualString);

            if (split != 2)
            {
                _logger.LogError("Malformed line: {Line}", line.ToString());
                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Malformed line: {Line}", line.ToString());
            return;
        }

        var instruction = line[ranges[0]];
        var instructionParts = instruction.Split('.');
        var instructionPartStrings = new List<string>();
        foreach (var (subStr, _) in instructionParts)
        {
            instructionPartStrings.Add(subStr.ToString());
        }

        var value = line[ranges[1]].Trim().ToString();

        // Pass result into processors
        foreach (var processor in _processors)
        {
            if (processor(config, instructionPartStrings, value)) return;
        }
    }
}
