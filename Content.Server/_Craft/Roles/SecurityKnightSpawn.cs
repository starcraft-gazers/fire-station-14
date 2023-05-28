
using System.Linq;
using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.Roles;

public sealed class SecurityKnightSpawn : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayersSpawned);
    }

    private void OnPlayersSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.JobId != "SecurityKnight" && ev.JobId != "Brigmedic")
            return;

        var attachedEntity = ev.Player.AttachedEntity;
        if (attachedEntity == null)
            return;

        var attachedTransform = Transform((EntityUid) attachedEntity);
        if (attachedTransform == null)
            return;

        var spawnPoints = _entityManager.EntityQuery<SpawnPointComponent>().ToList();
        if (spawnPoints == null)
            return;

        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.Job?.ID == "SecurityOfficer")
            {
                _transformSystem.SetCoordinates(attachedEntity.Value, Transform(spawnPoint.Owner).Coordinates);
                _transformSystem.AttachToGridOrMap(attachedEntity.Value, attachedTransform);

                break;
            }
        }
    }
}
