﻿using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;

public class RefTypeSettlementHouseAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition NoHouseContainerRefType = MutagenTopicBuilder.DevelopmentTopic(
            "No House Container Ref Type",
            Severity.Suggestion)
        .WithoutFormatting("Settlement house location has no House Container Ref Type - this is likely a mistake");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoHouseContainerRefType];

    private static readonly IReadOnlyList<IFormLinkGetter<ILocationReferenceTypeGetter>> HouseContainerRefTypes =
    [
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.HouseContainerRefType,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRWealthMarker01,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRWealthMarker02,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRWealthMarker03,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRWealthyHomeChest,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRLedger,
        FormKeys.SkyrimSE.Skyrim.LocationReferenceType.TGRSLStrongbox
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var location = param.Record;

        if (!location.IsSettlementLocation()) return;
        if (!location.IsSettlementHouseLocationNotPlayerHome()) return;
        if (!location.IsLocationAppliedToInterior(param.LinkCache)) return;

        if (location.GetReferenceTypes().Any(staticRef => HouseContainerRefTypes.Contains(staticRef.LocationRefType)))
        {
            return;
        }

        param.AddTopic(NoHouseContainerRefType.Format());
    }

    public IEnumerable<Func<ILocationGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.LocationCellUniques;
        yield return x => x.ActorCellStaticReferences;
    }
}
