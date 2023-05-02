namespace Content.Server.Spawners.EntitySystems;

public interface ISCPStationPointSystem
{
    public void Initialize();
    public bool IsSpawnPointAtSCPStation(EntityUid uid, TransformComponent xform);
}
