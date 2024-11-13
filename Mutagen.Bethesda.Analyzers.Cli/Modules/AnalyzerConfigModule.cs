using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Overrides;
using Mutagen.Bethesda.Analyzers.Config.Analyzer;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order.DI;

namespace Mutagen.Bethesda.Analyzers.Cli.Modules;

public class AnalyzerConfigModule(GameRelease gameRelease) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var analyzerConfig = GetAnalyzerConfig();

        if (analyzerConfig.DataDirectoryPath.HasValue)
        {
            var dataDirectoryProvider = new DataDirectoryInjection(analyzerConfig.DataDirectoryPath.Value);
            builder.RegisterInstance(dataDirectoryProvider).As<IDataDirectoryProvider>();
        }

        if (analyzerConfig.LoadOrderSetByDataDirectory)
        {
            builder.RegisterType<DataDirectoryEnabledPluginListingsProvider>().As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }
        else if (analyzerConfig.LoadOrderSetToMods is not null)
        {
            builder.RegisterInstance(new InjectedEnabledPluginListingsProvider(analyzerConfig.LoadOrderSetToMods)).As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }

        if (analyzerConfig.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(analyzerConfig.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }
    }

    private IAnalyzerConfigLookup GetAnalyzerConfig()
    {
        var services = new ServiceCollection();
        services.AddLogging(x => x.AddConsole());

        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterInstance(new FileSystem()).As<IFileSystem>();
        builder.RegisterModule<MainModule>();
        builder.RegisterInstance(new GameReleaseInjection(gameRelease))
            .AsImplementedInterfaces();

        var container = builder.Build();

        var analyzerConfigProvider = container.Resolve<AnalyzerConfigProvider>();
        return analyzerConfigProvider.Config;
    }
}
