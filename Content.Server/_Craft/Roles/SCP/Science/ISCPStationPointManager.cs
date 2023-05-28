namespace Content.Server.Spawners.EntitySystems;

public interface ISCPStationPointManager
{
    public void Initialize();
    public bool IsSpawnPointAtSCPStation(EntityUid uid, TransformComponent xform);
}
