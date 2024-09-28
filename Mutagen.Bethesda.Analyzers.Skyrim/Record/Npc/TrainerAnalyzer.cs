using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Results;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;
namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class TrainerAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition TrainerFactionMissingScript = MutagenTopicBuilder.DevelopmentTopic(
            "Trainer requires script",
            Severity.Warning)
        .WithoutFormatting("Npc is a trainer but does not have a TrainerGoldScript");

    public static readonly TopicDefinition TrainerScriptMissingFaction = MutagenTopicBuilder.DevelopmentTopic(
            "Trainer requires faction",
            Severity.Warning)
        .WithoutFormatting("Npc has TrainerGoldScript but is not in a trainer faction");

    public static readonly TopicDefinition TrainerWithoutSpecialization = MutagenTopicBuilder.DevelopmentTopic(
            "Trainer without specialization",
            Severity.Warning)
        .WithoutFormatting("NPCs with a JobMerchant faction need specialized Job faction, otherwise their merchant dialogue might not work");

    public IEnumerable<TopicDefinition> Topics { get; } = [TrainerFactionMissingScript, TrainerScriptMissingFaction, TrainerWithoutSpecialization];

    public RecordAnalyzerResult? AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        var result = new RecordAnalyzerResult();
        if (!npc.Template.IsNull) return null;

        var faction = npc.Factions
            .Select(x => x.Faction.TryResolve(param.LinkCache))
            .NotNull()
            .ToList();

        var hasTrainerGoldScript = npc.VirtualMachineAdapter is not null && npc.HasScript("TrainerGoldScript");
        var trainerFaction = faction.Find(x => x.EditorID?.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase) ?? false);

        if (hasTrainerGoldScript && trainerFaction is null)
        {
            result.AddTopic(
                RecordTopic.Create(
                    npc,
                    TrainerScriptMissingFaction.Format(),
                    x => x.VirtualMachineAdapter));
        }

        if (trainerFaction is not null && !hasTrainerGoldScript)
        {
            result.AddTopic(
                RecordTopic.Create(
                    npc,
                    TrainerFactionMissingScript.Format(),
                    x => x.Factions));
        }

        if (hasTrainerGoldScript || trainerFaction is not null)
        {
            var hasTrainerSpecialization = faction.Exists(f =>
                f.EditorID is not null
                && !f.EditorID.EndsWith("JobTrainer", StringComparison.OrdinalIgnoreCase)
                && f.EditorID.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase));

            if (!hasTrainerSpecialization)
            {
                result.AddTopic(
                    RecordTopic.Create(
                        npc,
                        TrainerWithoutSpecialization.Format(),
                        x => x.Factions));
            }
        }

        return result;
    }
}