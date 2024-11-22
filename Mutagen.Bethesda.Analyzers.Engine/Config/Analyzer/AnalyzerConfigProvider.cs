namespace Mutagen.Bethesda.Analyzers.Config.Analyzer;

public class AnalyzerConfigProvider
{
    private readonly Lazy<IAnalyzerConfigLookup> _config;
    public IAnalyzerConfigLookup Config => _config.Value;

    public AnalyzerConfigProvider(AnalyzerConfigBuilder builder)
    {
        _config = new Lazy<IAnalyzerConfigLookup>(builder.Build);
    }
}
