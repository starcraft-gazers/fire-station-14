using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;

namespace Content.Server._Craft.Administration.Commands.Fax;

[AdminCommand(AdminFlags.Adminhelp)]
public sealed class CreateCentralCommandFax : IConsoleCommand
{
    public string Command => "createcentralcommandfax";
    public string Description => "Spawn's map with Central Command Fax";

    public string Help => "Usage: createcentralcommandfax";

    public void Execute(IConsoleShell shell, string argsStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var centralCommandFaxSystem = entityManager.System<CreateCentralCommandFaxSystem>();

        TeleportPlayer(shell, centralCommandFaxSystem.GetFaxArea(), entityManager);
    }
    private void TeleportPlayer(IConsoleShell shell, EntityUid targetUid, IEntityManager entityManager)
    {
        var player = shell.Player as IPlayerSession;
        if (player?.Status != SessionStatus.InGame)
        {
            shell.WriteError("You need to be in game to teleport to an entity.");
            return;
        }

        if (!entityManager.TryGetComponent(player.AttachedEntity, out TransformComponent? playerTransform))
        {
            shell.WriteError("You don't have an entity.");
            return;
        }

        if (!entityManager.TryGetComponent(targetUid, out TransformComponent? targetTransform))
        {
            shell.WriteError("Can't find target to teleport. Is Area created?");
            return;
        }

        var transformSystem = entityManager.System<SharedTransformSystem>();
        var targetCoords = targetTransform.Coordinates;

        transformSystem.SetCoordinates(player.AttachedEntity!.Value, targetCoords);
        playerTransform.AttachToGridOrMap();
    }
}
