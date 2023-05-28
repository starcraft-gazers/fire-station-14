using Content.Server.Spawners.EntitySystems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Physics;

namespace Content.Server.Roles.SCP.Science;

public sealed class SCPStationPointManager : ISCPStationPointManager
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public void Initialize()
    {

    }

    public bool IsSpawnPointAtSCPStation(EntityUid uid, TransformComponent transform)
    {
        return _entityManager
            .System<SCPStationSystem>()
            .IsSpawnPointAtSCPStation(uid, transform);
    }
}
