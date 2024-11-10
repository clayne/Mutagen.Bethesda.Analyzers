using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using NSubstitute;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

public class TopicConfigReaderTests
{
    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning # A Comment")]
    public void TestSeverity(
        string line,
        ITopicConfig config,
        TopicConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.Received(1).Override(new TopicId("A", 123), Severity.Warning);
    }

    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning And Gibberish")]
    [AnalyzerInlineData("other.A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.1A123.severity = Warning")]
    [AnalyzerInlineData("diagnostic.A123")]
    [AnalyzerInlineData("diagnostic.A123.other = Warning")]
    [AnalyzerInlineData("diagnostic.A123.severity = Other")]
    [AnalyzerInlineData("diagnostic.A123.severity")]
    [AnalyzerInlineData("")]
    [AnalyzerInlineData("#diagnostic.A123.severity = Warning")]
    public void AbnormalLinesDontOverride(
        string line,
        ITopicConfig config,
        TopicConfigReader sut)
    {
        sut.ReadInto(line.AsSpan(), config);
        config.DidNotReceiveWithAnyArgs().Override(default!, default);
    }
}
