using System.Collections.Generic;
using System.Linq;
using Content.Server.Chemistry.Components.SolutionManager;
using Content.Server.Doors.Systems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Maps;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Doors.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Maps;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Chemistry.TileReactions;

[UsedImplicitly]
[DataDefinition]
public sealed class SCP173TileReaction : ITileReaction
{
    public FixedPoint2 TileReact(TileRef tile, ReagentPrototype reagent, FixedPoint2 reactVolume)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var lookupSystem = entityManager.System<EntityLookupSystem>();
        var entityCoordinates = tile.GridPosition();

        var entitiesInRange = lookupSystem.GetEntitiesInRange(entityCoordinates, 3);
        if (entitiesInRange != null)
        {
            TryOpenDoor(entityManager, lookupSystem, entitiesInRange);
        }

        var spillSystem = entityManager.System<PuddleSystem>();
        if (reactVolume < 5 || !spillSystem.TryGetPuddle(tile, out _))
            return FixedPoint2.Zero;

        return spillSystem.TrySpillAt(tile, new Solution(reagent.ID, reactVolume), out _, sound: false, tileReact: false)
            ? reactVolume
            : FixedPoint2.Zero;
    }

    private int GetSCPBloodVolume(IEntityManager entityManager, EntityLookupSystem lookupSystem, HashSet<EntityUid> entitiesInRange)
    {
        var puddles = entitiesInRange.Where(entity => entityManager.HasComponent<PuddleComponent>(entity));
        var volume = 0;

        foreach (var puddle in puddles)
        {
            if (!entityManager.TryGetComponent<SolutionContainerManagerComponent>(puddle, out var solutionContainer) || solutionContainer == null)
                continue;

            if (solutionContainer.Solutions.Count() <= 0)
                continue;

            var scpSolution = solutionContainer
                .Solutions
                .Where(solution => solution.Value.Name == "puddle")
                ?.First()
                .Value;

            if (scpSolution == null)
                continue;

            scpSolution.TryGetReagent("SCP173Blood", out var quantity);
            volume += quantity.Int();
        }

        return volume;
    }

    private void TryOpenDoor(IEntityManager entityManager, EntityLookupSystem lookupSystem, HashSet<EntityUid> entitiesInRange)
    {
        var volumesAround = GetSCPBloodVolume(entityManager, lookupSystem, entitiesInRange);
        if (volumesAround < 1700)
            return;

        var doorSystem = entityManager.System<DoorSystem>();
        var doors = entitiesInRange.Where(entity => entityManager.HasComponent<DoorComponent>(entity));

        foreach (var door in doors)
        {
            if (!entityManager.TryGetComponent<DoorComponent>(door, out var doorComp) || doorComp == null || doorComp.State != DoorState.Closed)
                continue;

            doorSystem.TryOpen(door, doorComp, quiet: true);
        }
    }
}
