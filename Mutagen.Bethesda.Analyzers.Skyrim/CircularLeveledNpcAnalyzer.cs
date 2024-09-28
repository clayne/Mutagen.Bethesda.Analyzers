﻿using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Results;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;
namespace Mutagen.Bethesda.Analyzers.Skyrim;

public partial class CircularLeveledListAnalyzer : IContextualRecordAnalyzer<ILeveledNpcGetter>
{
    public static readonly TopicDefinition<List<ILeveledNpcGetter>> CircularLeveledNpc = MutagenTopicBuilder.DevelopmentTopic(
            "Circular Leveled Npc",
            Severity.Suggestion)
        .WithFormatting<List<ILeveledNpcGetter>>("Leveled Npc contains itself in path {0}");

    public RecordAnalyzerResult AnalyzeRecord(ContextualRecordAnalyzerParams<ILeveledNpcGetter> param)
    {
        return FindCircularList(param.Record, l =>
        {
            if (l.Entries is not null)
            {
                return l.Entries
                    .Select(x => x.Data)
                    .NotNull()
                    .Select(x => x.Reference.FormKey);
            }

            return [];
        }, param.LinkCache, CircularLeveledNpc);
    }
}