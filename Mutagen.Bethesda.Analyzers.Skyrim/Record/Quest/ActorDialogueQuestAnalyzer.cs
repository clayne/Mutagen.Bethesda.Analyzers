﻿using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Results;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class ActorDialogueQuestAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition NoAliases = MutagenTopicBuilder.DevelopmentTopic(
            "No Aliases",
            Severity.Error)
        .WithoutFormatting("Quest has no aliases");

    public static readonly TopicDefinition OddNumberOfAliases = MutagenTopicBuilder.DevelopmentTopic(
            "Odd Number Of Aliases",
            Severity.Error)
        .WithoutFormatting("Quest has an odd number of aliases");

    public static readonly TopicDefinition<string?> AliasWithoutFindMatchingRefFromEvent = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without Find Matching Reference From Event",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} isn't Find Matching Reference From Event");

    public static readonly TopicDefinition<string?, int, int> AliasWithoutSameNumberOfConditionsAsNpcs = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without same number of conditions as npcs",
            Severity.Error)
        .WithFormatting<string?, int, int>("Alias {0} has {1} conditions, which doesn't match the {2} npcs in the scene");

    public static readonly TopicDefinition<string?, IEnumerable<Condition.Function>> AliasWithoutGetIsIDCondition = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without GetIsID condition",
            Severity.Error)
        .WithFormatting<string?, IEnumerable<Condition.Function>>("Alias {0} uses conditions with function {1} which are not GetIsID");

    public static readonly TopicDefinition<string?, int, int> AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without same number of GetIsID conditions as npcs",
            Severity.Error)
        .WithFormatting<string?, int, int>("Alias {0} has {1} GetIsID conditions, which doesn't match the {2} npcs in the scene");

    public static readonly TopicDefinition<string?, int> AliasWithoutGetDistanceCondition = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without GetDistance condition",
            Severity.Error)
        .WithFormatting<string?, int>("Alias {0} has {1} GetDistance conditions, not 1");

    public static readonly TopicDefinition<string?> AliasWithoutUniqueActor = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without Unique Actor",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} doesn't have a Unique Actor");

    public static readonly TopicDefinition<string?> AliasWithoutAllowReuseInQuestFlag = MutagenTopicBuilder.DevelopmentTopic(
            "Alias without Allow Reuse In Quest flag",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} doesn't have Allow Reuse In Quest flag");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        NoAliases,
        OddNumberOfAliases,
        AliasWithoutFindMatchingRefFromEvent,
        AliasWithoutSameNumberOfConditionsAsNpcs,
        AliasWithoutGetIsIDCondition,
        AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs,
        AliasWithoutGetDistanceCondition,
        AliasWithoutUniqueActor,
        AliasWithoutAllowReuseInQuestFlag
    ];

    public RecordAnalyzerResult? AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (!quest.Event.HasValue || quest.Event.Value != "ADIA") return null;

        var result = new RecordAnalyzerResult();

        if (quest.Aliases.Count == 0)
        {
            result.AddTopic(
                RecordTopic.Create(
                    quest,
                    NoAliases.Format(),
                    x => x.Aliases));

            return result;
        }

        if (quest.Aliases.Count % 2 != 0)
        {
            result.AddTopic(
                RecordTopic.Create(
                    quest,
                    OddNumberOfAliases.Format(),
                    x => x.Aliases));

            return result;
        }

        var firstAliasHalf = quest.Aliases.Take(quest.Aliases.Count / 2).ToList();
        var secondAliasHalf = quest.Aliases.Skip(quest.Aliases.Count / 2).ToList();

        var startsWithEventAlias = quest.Aliases[0].FindMatchingRefFromEvent is not null;
        var eventAliases = startsWithEventAlias ? firstAliasHalf : secondAliasHalf;
        for (var i = 0; i < eventAliases.Count; i++)
        {
            var eventAlias = eventAliases[i];
            if (i < 2)
            {
                if (eventAlias.FindMatchingRefFromEvent is null)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutFindMatchingRefFromEvent.Format(eventAlias.Name),
                            x => x.FindMatchingRefFromEvent));
                }

                if (eventAlias.Conditions.Count != eventAliases.Count)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutSameNumberOfConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count),
                            x => x.Conditions));
                }

                var conditions = eventAlias.Conditions.Where(condition => condition.Data is not IGetIsIDConditionDataGetter).ToList();
                if (conditions.Count > 0)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutGetIsIDCondition.Format(eventAlias.Name, conditions.Select(x => x.Data.Function).Distinct()),
                            x => x.Conditions));
                }
            }
            else
            {
                if (eventAlias.Conditions.Count != eventAliases.Count + 1)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutSameNumberOfConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count + 1),
                            x => x.Conditions));
                }

                if (eventAlias.Conditions.Count(condition => condition.Data is IGetIsIDConditionDataGetter) != eventAliases.Count)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count),
                            x => x.Conditions));
                }

                var count = eventAlias.Conditions.Count(condition => condition.Data is IGetDistanceConditionDataGetter);
                if (count != 1)
                {
                    result.AddTopic(
                        RecordTopic.Create(
                            eventAlias,
                            AliasWithoutGetDistanceCondition.Format(eventAlias.Name, count),
                            x => x.Conditions));
                }
            }
        }

        var npcAliases = startsWithEventAlias ? secondAliasHalf : firstAliasHalf;
        foreach (var npcAlias in npcAliases)
        {
            if (npcAlias.UniqueActor.IsNull)
            {
                result.AddTopic(
                    RecordTopic.Create(
                        npcAlias,
                        AliasWithoutUniqueActor.Format(npcAlias.Name),
                        x => x.UniqueActor));
            }
        }

        foreach (var alias in secondAliasHalf)
        {
            if (alias.Flags is null || !alias.Flags.Value.HasFlag(QuestAlias.Flag.AllowReuseInQuest))
            {
                result.AddTopic(
                    RecordTopic.Create(
                        alias,
                        AliasWithoutAllowReuseInQuestFlag.Format(alias.Name),
                        x => x.Flags));
            }
        }

        return result;
    }
}