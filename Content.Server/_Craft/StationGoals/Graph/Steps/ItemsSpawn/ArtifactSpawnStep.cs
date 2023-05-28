using System.Collections.Generic;
using Content.Server.Cargo.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Steps.ItemsSpawn;

[ImplicitDataDefinitionForInheritors]
public sealed class ArtifactSpawnStep : Step
{
    [DataField("artifactsSpawnerPrototype", serverOnly: true, required: true)]
    private readonly string ArtifactSpawnerPrototype = default!;

    [DataField("minArtifacts", serverOnly: true)]
    private readonly int MinArtifacts = 3;

    [DataField("maxArtifacts", serverOnly: true)]
    private readonly int MaxArtifacts = 6;

    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var shuttleUid = results.GetOrFallback(StepDataKey.SHUTTLE_UID, EntityUid.Invalid);
        if (shuttleUid == EntityUid.Invalid)
        {
            system.logger.RootSawmill.Debug($"Step: {Name} interrupted shuttleUid Invalid");
            return ExecuteState.Interrupted;
        }

        var entityManager = IoCManager.Resolve<IEntityManager>();
        var artifactsCount = IoCManager.Resolve<IRobustRandom>().Next(MinArtifacts, MaxArtifacts);
        var counter = 0;

        foreach (var (comp, compXform) in entityManager.EntityQuery<CargoPalletComponent, TransformComponent>(true))
        {
            if (counter == artifactsCount)
                break;

            if (compXform.ParentUid != shuttleUid || !compXform.Anchored)
                continue;


            entityManager.SpawnEntity(ArtifactSpawnerPrototype, compXform.Coordinates);
            counter++;
        }

        system.logger.RootSawmill.Debug($"Step: {Name} finished success");
        return ExecuteState.Finished;
    }
}
