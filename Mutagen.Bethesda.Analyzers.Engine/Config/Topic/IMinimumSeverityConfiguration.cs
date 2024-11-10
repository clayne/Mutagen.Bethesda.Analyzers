using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers.Config.Topic;

public interface IMinimumSeverityConfiguration
{
    Severity MinimumSeverity { get; }
}
