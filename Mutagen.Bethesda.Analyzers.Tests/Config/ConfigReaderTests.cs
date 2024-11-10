using FluentAssertions;
using Mutagen.Bethesda.Analyzers.Config;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Config;

public record TestConfig;

public class ConfigReaderTests
{
    [Theory]
    [AnalyzerInlineData("other.A123.severity = Warning")]
    public void TestThreePartAndValue(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(3);
                parts[0].Should().Be("other");
                parts[1].Should().Be("A123");
                parts[2].Should().Be("severity");
                value.Should().Be("Warning");
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeTrue();
    }

    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Warning And Gibberish")]
    public void TestThreePartAndValueWithSpaces(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(3);
                parts[0].Should().Be("diagnostic");
                parts[1].Should().Be("A123");
                parts[2].Should().Be("severity");
                value.Should().Be("Warning And Gibberish");
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeTrue();
    }


    [Theory]
    [AnalyzerInlineData("diagnostic.A123")]
    public void TestSkipNoValue(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(3);
                parts[0].Should().Be("diagnostic");
                parts[1].Should().Be("A123");
                value.Should().Be("");
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeFalse();
    }


    [Theory]
    [AnalyzerInlineData("")]
    public void TestSkipEmptyLine(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(0);
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeFalse();
    }


    [Theory]
    [AnalyzerInlineData("diagnostic.A123.severity = Other #test")]
    public void TestComment(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(3);
                parts[0].Should().Be("diagnostic");
                parts[1].Should().Be("A123");
                parts[2].Should().Be("severity");
                value.Should().Be("Other");
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeTrue();
    }


    [Theory]
    [AnalyzerInlineData("#diagnostic.A123.severity = Warning")]
    public void TestSkipJustComment(
        string line,
        ConfigReader<TestConfig> sut)
    {
        var ran = false;

        sut.Register((config, parts, value) =>
            {
                ran = true;
                parts.Should().HaveCount(0);
                return true;
            })
            .ReadInto(line.AsSpan(), new TestConfig());

        ran.Should().BeFalse();
    }
}
