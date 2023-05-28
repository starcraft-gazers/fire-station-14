using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server._Craft.StationGoals.Graph.Steps;
using Content.Server._Craft.Utils;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Coordinates.Helpers;
using Content.Server.Procedural;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Components;
using Content.Shared.Gravity;
using Content.Shared.Procedural;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Shuttle.Dungeons;

public sealed class DungeonsGenerator : Step
{
    [DataField("DungeonPrototypeID", serverOnly: true, required: true)]
    private string DungeonPrototypeID = default!;

    [DataField("EnemiesPrototypes", serverOnly: true, required: false)]
    private string[] EnemiesPrototypes = Array.Empty<string>();

    [DataField("EnemiesCount", serverOnly: true)]
    private int EnemiesCount = 10;

    private MapId MapId = MapId.Nullspace;
    private EntityUid DungeonUid = EntityUid.Invalid;

    private MapGridComponent? DungeonGrid = default!;

    private Vector2i DungeonPosition = new Vector2i(30, 30);
    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var dependencies = new Dependencies(
            entityManager,
            IoCManager.Resolve<IMapManager>(),
            entityManager.System<DungeonSystem>(),
            entityManager.System<MapLoaderSystem>(),
            IoCManager.Resolve<IPrototypeManager>(),
            IoCManager.Resolve<IRobustRandom>(),
            IoCManager.Resolve<ITileDefinitionManager>(),
            entityManager.System<AtmosphereSystem>()
        );

        if (!PrepareToGeneration(dependencies, out var dungeon) || dungeon == null || DungeonGrid == null)
        {
            return ExecuteState.Interrupted;
        }

        GenerateDungeon(dungeon, dependencies);

        return ExecuteState.Finished;
    }

    private bool PrepareToGeneration(Dependencies dependencies, out DungeonConfigPrototype? dungeon)
    {
        var mapManager = dependencies.MapManager;
        var entityManager = dependencies.EntityManager;
        var prototypeManager = dependencies.PrototypeManager;

        if (!prototypeManager.TryIndex<DungeonConfigPrototype>(DungeonPrototypeID, out dungeon))
        {
            return false;
        }

        MapId = mapManager.CreateMap();
        DungeonUid = mapManager.GetMapEntityId(MapId);
        if (!entityManager.TryGetComponent<MapGridComponent>(DungeonUid, out DungeonGrid))
        {
            DungeonUid = entityManager.CreateEntityUninitialized(null, new EntityCoordinates(DungeonUid, DungeonPosition));
            DungeonGrid = entityManager.AddComponent<MapGridComponent>(DungeonUid);
            entityManager.InitializeAndStartEntity(DungeonUid, MapId);
        }

        return true;
    }

    private async void GenerateDungeon(DungeonConfigPrototype dungeon, Dependencies dependencies)
    {
        var seed = dependencies.RobustRandom.Next(1000, 20000000);
        await dependencies.DungeonSystem.GenerateDungeonAsync(dungeon, DungeonUid, DungeonGrid!, DungeonPosition, seed);

        AddFTLDestination(dependencies);
        SetupMetaData(dependencies);
        SpawnXenos(dependencies);
    }

    private void AddFTLDestination(Dependencies dependencies)
    {
        var entityManager = dependencies.EntityManager;

        var ftlComponent = entityManager.EnsureComponent<FTLDestinationComponent>(DungeonUid);
        ftlComponent.Enabled = true;
        ftlComponent.Whitelist = null;
    }

    private void SetupMetaData(Dependencies dependencies)
    {
        var metaDataComponent = dependencies.EntityManager.EnsureComponent<MetaDataComponent>(DungeonUid);
        metaDataComponent.EntityName = "Неизвестный объект";
    }

    private void SpawnXenos(Dependencies dependencies)
    {
        for (int i = 1; i < EnemiesCount; i++)
        {
            if (CoordinationUtils.TryFindRandomSaveTile(DungeonUid, DungeonUid, dependencies.MapManager, dependencies.EntityManager, dependencies.TileDefinitions, dependencies.AtmosphereSystem, dependencies.RobustRandom, 10, out var coords))
            {
                var randomMob = dependencies.RobustRandom.Pick<string>(EnemiesPrototypes);
                if (randomMob == null)
                {
                    continue;
                }

                dependencies.EntityManager.SpawnEntity(randomMob, coords.SnapToGrid());
            }
        }
    }

    internal record class Dependencies
    {
        public IEntityManager EntityManager;
        public IMapManager MapManager;
        public DungeonSystem DungeonSystem;
        public IPrototypeManager PrototypeManager;
        public MapLoaderSystem MapLoaderSystem;
        public IRobustRandom RobustRandom;
        public ITileDefinitionManager TileDefinitions;
        public AtmosphereSystem AtmosphereSystem;

        internal Dependencies(
            IEntityManager entityManager,
            IMapManager mapManager,
            DungeonSystem dungeonSystem,
            MapLoaderSystem mapLoaderSystem,
            IPrototypeManager prototypeManager,
            IRobustRandom robustRandom,
            ITileDefinitionManager tileDefinitions,
            AtmosphereSystem atmosphereSystem
        )
        {
            EntityManager = entityManager;
            MapManager = mapManager;
            DungeonSystem = dungeonSystem;
            MapLoaderSystem = mapLoaderSystem;
            PrototypeManager = prototypeManager;
            RobustRandom = robustRandom;
            TileDefinitions = tileDefinitions;
            AtmosphereSystem = atmosphereSystem;
        }
    }
}
