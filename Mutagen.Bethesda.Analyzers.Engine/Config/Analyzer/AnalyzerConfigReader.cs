using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Plugins;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class AnalyzerConfigReader : IConfigReader<IAnalyzerConfig>
{
    private const string EnvironmentGroup = "environment";

    private readonly ConfigReader<IAnalyzerConfig> _reader;
    private readonly ILogger<AnalyzerConfigReader> _logger;

    public AnalyzerConfigReader(
        ConfigReader<IAnalyzerConfig> reader,
        ILogger<AnalyzerConfigReader> logger)
    {
        _logger = logger;
        _reader = reader
            .Register(ProcessDataDirectoryPath)
            .Register(ProcessLoadOrderSetByDataDirectory)
            .Register(ProcessLoadOrderSetToMods)
            .Register(ProcessGameRelease)
            .Register(ProcessOutputFilePath);
    }

    public void ReadInto(FilePath path, IAnalyzerConfig config) => _reader.ReadInto(path, config);
    public void ReadInto(ReadOnlySpan<char> line, IAnalyzerConfig config) => _reader.ReadInto(line, config);

    private bool ProcessDataDirectoryPath(IAnalyzerConfig config, IReadOnlyList<string> instructions, string value)
    {
        // environment.data_directory = <path>
        if (instructions.Count != 2) return false;

        if (instructions[0] is not EnvironmentGroup) return false;
        if (instructions[1] is not "data_directory") return false;

        config.OverrideDataDirectory(value);

        return true;
    }

    private bool ProcessLoadOrderSetByDataDirectory(IAnalyzerConfig config, IReadOnlyList<string> instructions, string value)
    {
        // environment.load_order.set_by_data_directory = <bool>
        if (instructions.Count != 3) return false;

        if (instructions[0] is not EnvironmentGroup) return false;
        if (instructions[1] is not "load_order") return false;
        if (instructions[2] is not "set_by_data_directory") return false;

        if (!bool.TryParse(value, out var setByDataDirectory))
        {
            _logger.LogError("Error parsing LoadOrderSetByDataDirectory");
            return false;
        }

        config.OverrideLoadOrderSetByDataDirectory(setByDataDirectory);

        return true;
    }

    private bool ProcessLoadOrderSetToMods(IAnalyzerConfig config, IReadOnlyList<string> instructions, string value)
    {
        // environment.load_order.set_to_mods = <mod1>,<mod2>,...
        if (instructions.Count != 3) return false;

        if (instructions[0] is not EnvironmentGroup) return false;
        if (instructions[1] is not "load_order") return false;
        if (instructions[2] is not "set_to_mods") return false;

        var mods = value.Split(',');
        try
        {
            var modKeys = mods.Select(fileName => ModKey.FromFileName(fileName.Trim())).ToList();
            config.OverrideLoadOrderSetToMods(modKeys);
        }
        catch (ArgumentException e)
        {
            _logger.LogError(e, "Error parsing ModKeys");
            return false;
        }

        return true;
    }

    private bool ProcessGameRelease(IAnalyzerConfig config, IReadOnlyList<string> instructions, string value)
    {
        // environment.game_release = <release>
        if (instructions.Count != 2) return false;

        if (instructions[0] is not EnvironmentGroup) return false;
        if (instructions[1] is not "game_release") return false;

        if (!Enum.TryParse<GameRelease>(value, out var release))
        {
            _logger.LogError("Error parsing GameRelease");
            return false;
        }

        config.OverrideGameRelease(release);

        return true;
    }

    private bool ProcessOutputFilePath(IAnalyzerConfig config, IReadOnlyList<string> instructions, string value)
    {
        // output_file = <path>
        if (instructions.Count != 1) return false;

        if (instructions[0] is not "output_file") return false;

        config.OverrideOutputFilePath(value);

        return true;
    }
}
