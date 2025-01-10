using System.IO.Abstractions;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Overrides;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins.Order.DI;

namespace Mutagen.Bethesda.Analyzers.Cli.Modules;

public class RunConfigModule(GameRelease gameRelease) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var runConfig = GetRunConfig();

        if (runConfig.DataDirectoryPath.HasValue)
        {
            var dataDirectoryProvider = new DataDirectoryInjection(runConfig.DataDirectoryPath.Value);
            builder.RegisterInstance(dataDirectoryProvider).As<IDataDirectoryProvider>();
        }

        if (runConfig.LoadOrderSetToMods is not null)
        {
            builder.RegisterInstance(new InjectedEnabledPluginListingsProvider(runConfig.LoadOrderSetToMods)).As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }

        if (runConfig.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(runConfig.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }
    }

    private IRunConfigLookup GetRunConfig()
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

        var runConfigProvider = container.Resolve<RunConfigProvider>();
        return runConfigProvider.Config;
    }
}
