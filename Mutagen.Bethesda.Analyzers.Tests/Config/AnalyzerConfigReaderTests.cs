using Mutagen.Bethesda.Analyzers.Config.Analyzer;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Noggog;
using NSubstitute;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

public class AnalyzerConfigReaderTests
{
    [Theory]
    [AnalyzerInlineData("environment.data_directory = C:/some/path")]
    [AnalyzerInlineData(@"environment.data_directory = C:\some\path")]
    public void TestDataDirectory(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideDataDirectory("C:/some/path");
    }

    [Theory]
    [AnalyzerInlineData("environment.load_order.set_by_data_directory = true")]
    [AnalyzerInlineData("environment.load_order.set_by_data_directory = false")]
    public void TestSetByDataDirectory(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideLoadOrderSetByDataDirectory(Arg.Any<bool>());
    }

    [Theory]
    [AnalyzerInlineData("environment.load_order.set_to_mods = Skyrim.esm, Update.esm")]
    public void TestSetToMods(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        List<ModKey> modKeys = [FormKeys.SkyrimSE.Skyrim.ModKey, Update.ModKey];
        config.Received(1).OverrideLoadOrderSetToMods(Arg.Is<List<ModKey>>(list => list.SequenceEqual(modKeys)));
    }

    [Theory]
    [AnalyzerInlineData("environment.game_release = SkyrimSE")]
    [AnalyzerInlineData("environment.game_release = Fallout4")]
    [AnalyzerInlineData("environment.game_release = Starfield")]
    public void TestGameRelease(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideGameRelease(Arg.Any<GameRelease>());
    }

    [Theory]
    [AnalyzerInlineData("output_file = C:/some/path")]
    [AnalyzerInlineData(@"output_file = C:\some\path")]
    public void TestOutputFilePath(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideOutputFilePath(Arg.Any<FilePath>());
    }
}
