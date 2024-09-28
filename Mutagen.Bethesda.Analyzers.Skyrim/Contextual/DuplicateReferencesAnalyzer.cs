﻿using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Results;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;
namespace Mutagen.Bethesda.Analyzers.Skyrim.Contextual;

public class DuplicateReferencesAnalyzer : IContextualAnalyzer
{
    public static readonly TopicDefinition<List<IPlacedObjectGetter>, List<IPlacedObjectGetter>> DuplicateReferences = MutagenTopicBuilder.DevelopmentTopic(
            "Duplicate References",
            Severity.Suggestion)
        .WithFormatting<List<IPlacedObjectGetter>, List<IPlacedObjectGetter>>("The following references are duplicate records of {0} and can be deleted: {1}");

    private static readonly FuncEqualityComparer<IPlacedObjectGetter> DuplicatePlacedComparer = new(
        (a, b) =>
        {
            if (a is null || b is null) return false;
            if (ReferenceEquals(a, b)) return true;

            return a.Placement!.Equals(b.Placement) && a.Scale.Equals(b.Scale) && a.Base.FormKey.Equals(b.Base.FormKey);
        },
        lookup => HashCode.Combine(lookup.Placement, lookup.Scale, lookup.Base.FormKey));

    public IEnumerable<TopicDefinition> Topics { get; } = [DuplicateReferences];

    public ContextualAnalyzerResult Analyze(ContextualAnalyzerParams param)
    {
        var result = new ContextualAnalyzerResult();

        foreach (var cell in param.LinkCache.PriorityOrder.WinningOverrides<ICellGetter>())
        {
            // Group all placed objects by their placement and scale
            var duplicateGroups = cell.GetAllPlaced(param.LinkCache)
                .Where(placed => placed is { IsDeleted: false, Placement: not null })
                .OfType<IPlacedObjectGetter>()
                .GroupBy(x => x, DuplicatePlacedComparer);

            foreach (var duplicateGroup in duplicateGroups)
            {
                var duplicates = duplicateGroup.ToArray();
                if (duplicates.Length <= 1) continue;

                // TODO: Exclude any placed objects with references to it
                var dispensableDuplicates = duplicates
                    .Where(placed => placed is { VirtualMachineAdapter: null, EnableParent: null, NavigationDoorLink: null, Patrol: null, LinkedReferences.Count: 0 })
                    .Where(placed => placed.SkyrimMajorRecordFlags.HasFlag((SkyrimMajorRecord.SkyrimMajorRecordFlag) PlacedObject.DefaultMajorFlag.Persistent))
                    .ToList();

                // All duplicates are indispensable
                if (dispensableDuplicates.Count == 0) continue;

                // Keep the first duplicate
                List<IPlacedObjectGetter> keptDuplicates;
                List<IPlacedObjectGetter> removedDuplicates;
                if (dispensableDuplicates.Count == duplicates.Length)
                {
                    keptDuplicates = [dispensableDuplicates[0]];
                    removedDuplicates = dispensableDuplicates.Skip(1).ToList();
                }
                else
                {
                    keptDuplicates = duplicates.Except(dispensableDuplicates).ToList();
                    removedDuplicates = dispensableDuplicates;
                }

                result.AddTopic(
                    ContextualTopic.Create(
                        cell,
                        DuplicateReferences.Format(keptDuplicates, removedDuplicates)
                    )
                );
            }
        }

        return result;
    }
}