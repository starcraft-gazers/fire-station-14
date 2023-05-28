using Content.Server.GameTicking;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.Server._Craft.Utils;

public static class ShuttleUtils
{
    public static (MapId, EntityUid) CreateShuttleOnNewMap(IMapManager mapManager, MapLoaderSystem mapSystem, IEntityManager entityManager, string shuttlePath)
    {
        return CreateShuttleOnNewMap(mapManager, mapSystem, entityManager, shuttlePath, 0, 0);
    }

    public static (MapId, EntityUid) CreateShuttleOnNewMap(IMapManager mapManager, MapLoaderSystem mapSystem, IEntityManager entityManager, string shuttlePath, int xOffset = 0, int yOffset = 0)
    {
        MapId mapId = MapId.Nullspace;
        EntityUid shuttleUid = EntityUid.Invalid;

        mapId = mapManager.CreateMap();
        if (mapId == MapId.Nullspace || !mapSystem.TryLoad(mapId, shuttlePath, out var gridList, GetMapLoadOptions(xOffset, yOffset)) || gridList == null)
        {
            return (mapId, shuttleUid);
        }

        shuttleUid = gridList[0];
        entityManager.EnsureComponent<ShuttleComponent>(shuttleUid);
        mapManager.SetMapPaused(mapId, false);

        return (mapId, shuttleUid);
    }

    private static MapLoadOptions? GetMapLoadOptions(int xOffset, int yOffset)
    {
        return new MapLoadOptions()
        {
            Offset = new Vector2(xOffset, yOffset)
        };
    }

    public static EntityUid CreateShuttleOnExistedMap(MapId mapId, IMapManager mapManager, MapLoaderSystem mapSystem, IEntityManager entityManager, string shuttlePath)
    {
        return CreateShuttleOnExistedMap(mapId, mapManager, mapSystem, entityManager, shuttlePath, 0, 0);
    }

    public static EntityUid CreateShuttleOnExistedMap(MapId mapId, IMapManager mapManager, MapLoaderSystem mapSystem, IEntityManager entityManager, string shuttlePath, int xOffset, int yOffset)
    {
        EntityUid shuttleUid = EntityUid.Invalid;

        if (mapId == MapId.Nullspace || !mapSystem.TryLoad(mapId, shuttlePath, out var gridList, GetMapLoadOptions(xOffset, yOffset)) || gridList == null)
        {
            return shuttleUid;
        }

        shuttleUid = gridList[0];
        entityManager.EnsureComponent<ShuttleComponent>(shuttleUid);
        mapManager.SetMapPaused(mapId, false);

        return shuttleUid;
    }


    public static EntityUid GetTargetStation(GameTicker ticker, IMapManager mapManager, IEntityManager entityManager)
    {
        var _targetmap = ticker.DefaultMap;
        EntityUid targetStation = EntityUid.Invalid;
        foreach (var grid in mapManager.GetAllMapGrids(_targetmap))
        {
            if (!entityManager.TryGetComponent<StationMemberComponent>(grid.Owner, out var stationMember)) continue;
            if (!entityManager.TryGetComponent<StationDataComponent>(stationMember.Station, out var stationData)) continue;
            targetStation = stationMember.Station;
            break;
        }

        return targetStation;
    }
}
