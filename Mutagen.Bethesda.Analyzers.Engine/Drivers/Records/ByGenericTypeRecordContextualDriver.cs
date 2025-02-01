﻿using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Drivers.Records;

public class ByGenericTypeRecordContextualDriver<TMajor> : IContextualDriver
    where TMajor : class, IMajorRecordGetter
{
    private readonly IWorkDropoff _dropoff;
    private readonly IContextualRecordAnalyzer<TMajor>[] _contextualRecordAnalyzers;

    public bool Applicable => _contextualRecordAnalyzers.Length > 0;

    public IEnumerable<IAnalyzer> Analyzers => _contextualRecordAnalyzers;

    public ByGenericTypeRecordContextualDriver(
        IAnalyzerProvider<IContextualRecordAnalyzer<TMajor>> contextualRecordAnalyzerProvider,
        IWorkDropoff dropoff)
    {
        _contextualRecordAnalyzers = contextualRecordAnalyzerProvider.GetAnalyzers().ToArray();
        _dropoff = dropoff;
    }

    public async Task Drive(ContextualDriverParams driverParams)
    {
        if (driverParams.CancellationToken.IsCancellationRequested) return;
        if (_contextualRecordAnalyzers.Length == 0) return;
        foreach (var listing in driverParams.LoadOrder.ListedOrder)
        {
            if (driverParams.CancellationToken.IsCancellationRequested) return;
            if (listing.Mod is null) continue;

            await Task.WhenAll(listing.Mod.EnumerateMajorRecords<TMajor>().SelectMany(rec =>
            {
                var param = new ContextualRecordAnalyzerParams<TMajor>(
                    driverParams.LinkCache,
                    driverParams.LoadOrder,
                    listing.Mod,
                    rec,
                    driverParams.ReportDropbox);
                return _contextualRecordAnalyzers.Select(analyzer =>
                {
                    return _dropoff.EnqueueAndWait(() =>
                    {
                        analyzer.AnalyzeRecord(param with
                        {
                            AnalyzerType = analyzer.GetType()
                        });
                    }, driverParams.CancellationToken);
                });
            }));
        }
    }
}
