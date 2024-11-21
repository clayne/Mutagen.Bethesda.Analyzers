using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Config;
using Mutagen.Bethesda.Analyzers.Config.Analyzer;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using NSubstitute;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

[method: SuppressMessage("ReSharper", "ContextualLoggerProblem", Justification = "Passed in")]
public class AnalyzerConfigReader(
    ILogger<ConfigReader<IAnalyzerConfig>> logger,
    ProcessDataDirectoryPath p1,
    ProcessGameRelease p2,
    ProcessLoadOrderSetByDataDirectory p3,
    ProcessLoadOrderSetToMods p4,
    ProcessOutputFilePath p5)
{
    public readonly ConfigReader<IAnalyzerConfig> Reader = new(logger, [p1, p2, p3, p4, p5]);
}

public class AnalyzerConfigReaderTests
{
    [Theory]
    [AnalyzerInlineData("environment.data_directory = C:/some/path")]
    [AnalyzerInlineData(@"environment.data_directory = C:\some\path")]
    public void TestDataDirectory(
        string line,
        AnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.DataDirectoryPath.Should().NotBeNull();
        config.DataDirectoryPath!.Value.Path.Should().Be(@"C:\some\path");
    }

    [Theory]
    [AnalyzerInlineData("environment.load_order.set_by_data_directory = true")]
    [AnalyzerInlineData("environment.load_order.set_by_data_directory = false")]
    public void TestSetByDataDirectory(
        string line,
        IAnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideLoadOrderSetByDataDirectory(Arg.Any<bool>());
    }

    [Theory]
    [AnalyzerInlineData("environment.load_order.set_to_mods = Skyrim.esm, Update.esm")]
    public void TestSetToMods(
        string line,
        AnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.LoadOrderSetToMods.Should().BeEquivalentTo([FormKeys.SkyrimSE.Skyrim.ModKey, Update.ModKey]);
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
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.Received(1).OverrideGameRelease(Arg.Any<GameRelease>());
    }

    [Theory]
    [AnalyzerInlineData("output_file = C:/some/path")]
    [AnalyzerInlineData(@"output_file = C:\some\path")]
    public void TestOutputFilePath(
        string line,
        AnalyzerConfig config,
        AnalyzerConfigReader sut)
    {
        sut.Reader.ReadInto(line.AsSpan(), config);
        config.OutputFilePath.Should().NotBeNull();
        config.OutputFilePath!.Value.Path.Should().Be(@"C:\some\path");
    }
}
