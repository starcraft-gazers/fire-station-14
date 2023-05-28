using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server._Craft.Administration.Commands.ERT;

[AdminCommand(AdminFlags.Adminhelp)]
public sealed class ERTSpawnCommand : IConsoleCommand
{
    public string Command => "ertspawn";
    public string Description => "Spawn's ERT Team to safe station";
    public string Help => "Usage: ertspawn";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var ertSystem= entityManager.System<ERTSystem>();

        ertSystem.CallERT();
    }
}

