using Autofac;
using Mutagen.Bethesda.Analyzers.Cli.Args;
using Mutagen.Bethesda.Analyzers.Cli.Overrides;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Order.DI;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Cli.Modules;

public class AnalyzerCommandModule(RunAnalyzersCommand command) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterInstance(command).AsImplementedInterfaces();
        builder.RegisterInstance(new GameReleaseInjection(command.GameRelease)).AsImplementedInterfaces();
        builder.RegisterInstance(new NumWorkThreadsConstant(command.NumThreads)).AsImplementedInterfaces();

        if (command.OutputFilePath is not null)
        {
            builder.RegisterType<CsvReportHandler>().AsImplementedInterfaces().SingleInstance();
            builder.RegisterInstance(new CsvInputs(command.OutputFilePath)).AsSelf().AsImplementedInterfaces();
        }

        if (command.CustomDataFolder is not null)
        {
            var dataDirectoryProvider = new DataDirectoryInjection(command.CustomDataFolder);
            builder.RegisterInstance(dataDirectoryProvider).As<IDataDirectoryProvider>();
        }

        if (command.LoadOrder is not null)
        {
            var loadOrder = command.LoadOrder.Split(',')
                .Select(x => x.Trim())
                .Select(x => ModKey.FromFileName(x));

            builder.RegisterInstance(new InjectedEnabledPluginListingsProvider(loadOrder)).As<IEnabledPluginListingsProvider>();
            builder.RegisterType<NullPluginListingsPathProvider>().As<IPluginListingsPathProvider>();
            builder.RegisterType<NullCreationClubListingsPathProvider>().As<ICreationClubListingsPathProvider>();
        }
    }
}
