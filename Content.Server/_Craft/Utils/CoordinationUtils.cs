using Robust.Shared.Map;
using Robust.Shared.Random;
using Content.Shared.Maps;
using Content.Server.Coordinates.Helpers;
using Robust.Shared.Physics.Systems;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;

namespace Content.Server._Craft.Utils;

public static class CoordinationUtils
{
    public static bool TryFindRandomSaveTile(
        EntityUid targetEntity,
        EntityUid targetMap,
        IMapManager mapManager,
        IEntityManager entityManager,
        ITileDefinitionManager tileDefinitionManager,
        AtmosphereSystem atmosphereSystem,
        IRobustRandom random,
        int maxAttempts,
        out EntityCoordinates targetCoords
        )
    {
        targetCoords = EntityCoordinates.Invalid;

        if (!mapManager.TryGetGrid(targetEntity, out var grid))
            return false;

        var xform = entityManager.GetComponent<TransformComponent>(targetEntity);

        if (!grid.TryGetTileRef(xform.Coordinates, out var tileRef))
            return false;

        var tile = tileRef.GridIndices;

        var found = false;
        var (gridPos, _, gridMatrix) = xform.GetWorldPositionRotationMatrix();
        var gridBounds = gridMatrix.TransformBox(grid.LocalAABB);

        for (var i = 0; i < maxAttempts; i++)
        {
            var randomX = random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = random.Next((int) gridBounds.Bottom, (int) gridBounds.Top);

            tile = new Vector2i(randomX - (int) gridPos.X, randomY - (int) gridPos.Y);
            var mapPos = grid.GridTileToWorldPos(tile);
            var mapTarget = grid.WorldToTile(mapPos);
            var circle = new Circle(mapPos, 2);

            foreach (var newTileRef in grid.GetTilesIntersecting(circle, true))
            {
                if (newTileRef.IsSpace(tileDefinitionManager) || newTileRef.IsBlockedTurf(true) || IsColliding(newTileRef.GridPosition(mapManager), entityManager))
                    continue;

                found = true;
                targetCoords = grid.GridTileToLocal(tile).SnapToGrid(entityManager, mapManager);
                break;
            }

            if (found)
                break;
        }

        if (!found)
            return false;

        return true;
    }

    private static bool IsColliding(EntityCoordinates coordinates, IEntityManager entityManager)
    {
        var mapCoords = coordinates.ToMap(entityManager);
        var (x, y) = mapCoords.Position;

        var collisionBox = Box2.FromDimensions(x, y, 0f, 0f);

        return entityManager.System<SharedPhysicsSystem>().TryCollideRect(collisionBox, mapCoords.MapId);
    }
}
