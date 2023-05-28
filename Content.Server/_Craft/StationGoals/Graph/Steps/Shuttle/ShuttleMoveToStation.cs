using System.Collections.Generic;
using Content.Server._Craft.Utils;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Steps.Shuttle;

[ImplicitDataDefinitionForInheritors]
public sealed class ShuttleMoveToStation : Step
{
    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var shuttleUid = results.GetOrFallback<EntityUid>(StepDataKey.SHUTTLE_UID, EntityUid.Invalid);
        if (shuttleUid == EntityUid.Invalid)
        {
            system.logger.RootSawmill.Debug($"Step: {Name} interrupted shuttleUid Invalid");
            return ExecuteState.Interrupted;
        }

        var mapManager = IoCManager.Resolve<IMapManager>();
        var entitySystemManager = IoCManager.Resolve<IEntitySystemManager>();
        var shuttleSystem = entitySystemManager.GetEntitySystem<ShuttleSystem>();
        var gameTicker = entitySystemManager.GetEntitySystem<GameTicker>();
        var stationSystem = entitySystemManager.GetEntitySystem<StationSystem>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();


        var targetStation = ShuttleUtils.GetTargetStation(gameTicker, mapManager, entityManager);
        if (targetStation == EntityUid.Invalid)
        {
            system.logger.RootSawmill.Debug($"Step: {Name} interrupted targetStation Invalid");
            return ExecuteState.Interrupted;
        }

        var targetGrid = stationSystem.GetLargestGrid(entityManager.GetComponent<StationDataComponent>(targetStation)).GetValueOrDefault();

        var shuttleComponent = entityManager.EnsureComponent<ShuttleComponent>(shuttleUid);
        shuttleSystem.TryFTLDock(
            shuttleUid: shuttleUid,
            component: shuttleComponent,
            targetUid: targetGrid
        );

        system.logger.RootSawmill.Debug($"Step: {Name} finished success");
        return ExecuteState.Finished;
    }
}
