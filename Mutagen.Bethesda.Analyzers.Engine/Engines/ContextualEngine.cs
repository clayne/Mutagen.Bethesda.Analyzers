﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutagen.Bethesda.Analyzers.Drivers;
using Mutagen.Bethesda.Analyzers.Reporting;
using Mutagen.Bethesda.Environments.DI;

namespace Mutagen.Bethesda.Analyzers.Engines
{
    public interface IContextualEngine : IEngine
    {
        void Run(IReportDropbox reportDropbox);
    }

    public class ContextualEngine : IContextualEngine
    {
        public IGameEnvironmentProvider EnvGetter { get; }
        public IDataDirectoryProvider DataDirectoryProvider { get; }
        public IDriverProvider<IContextualDriver> ContextualModDrivers { get; }
        public IDriverProvider<IIsolatedDriver> IsolatedModDrivers { get; }

        public IEnumerable<IDriver> Drivers => ContextualModDrivers.Drivers
            .Concat<IDriver>(IsolatedModDrivers.Drivers);

        public ContextualEngine(
            IGameEnvironmentProvider envGetter,
            IDataDirectoryProvider dataDataDirectoryProvider,
            IDriverProvider<IContextualDriver> contextualDrivers,
            IDriverProvider<IIsolatedDriver> isolatedDrivers)
        {
            EnvGetter = envGetter;
            DataDirectoryProvider = dataDataDirectoryProvider;
            ContextualModDrivers = contextualDrivers;
            IsolatedModDrivers = isolatedDrivers;
        }

        public void Run(IReportDropbox reportDropbox)
        {
            using var env = EnvGetter.Construct();

            var contextualParam = new ContextualDriverParams(
                env.LinkCache,
                env.LoadOrder,
                reportDropbox);

            var isolatedDrivers = IsolatedModDrivers.Drivers;
            if (isolatedDrivers.Count > 0)
            {
                foreach (var listing in env.LoadOrder.ListedOrder)
                {
                    if (listing.Mod == null) continue;

                    var modPath = Path.Combine(DataDirectoryProvider.Path, listing.ModKey.FileName);

                    var isolatedParam = new IsolatedDriverParams(
                        listing.Mod.ToUntypedImmutableLinkCache(),
                        reportDropbox,
                        listing.Mod,
                        modPath);

                    foreach (var driver in IsolatedModDrivers.Drivers)
                    {
                        driver.Drive(isolatedParam);
                    }
                }
            }

            foreach (var driver in ContextualModDrivers.Drivers)
            {
                driver.Drive(contextualParam);
            }
        }
    }
}
