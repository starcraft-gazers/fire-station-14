using System.Linq;
using Content.Server.Administration;
using Content.Server.Popups;
using Content.Shared.Administration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Player;

namespace Content.Server._Craft.Administration;

[AdminCommand(AdminFlags.Host)]
public sealed class ShowPopupToUser : IConsoleCommand
{
    public string Command => "showpopup";
    public string Description => "Показать попап игроку";
    public string Help => "showpopup ID text";

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var locator = IoCManager.Resolve<IPlayerLocator>();
        var playerManager = IoCManager.Resolve<IPlayerManager>();
        var popupSystem = entityManager.System<PopupSystem>();

        if (args.Length < 2)
        {
            shell.WriteLine($"Invalid amount of arguments.{Help}");
            return;
        }

        var data = await locator.LookupIdByNameOrIdAsync(args[0]);
        if (data == null)
            return;

        var player = playerManager.GetSessionByUserId(data.UserId);
        if (player.AttachedEntity != null)
        {
            var entityUid = (EntityUid) player.AttachedEntity;
            popupSystem.PopupEntity(args[1], entityUid, Filter.Entities(entityUid), true);
        }
    }
    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var playerMgr = IoCManager.Resolve<IPlayerManager>();
            var options = playerMgr.ServerSessions.Select(c => c.Name).OrderBy(c => c).ToArray();
            return CompletionResult.FromHintOptions(options, "Имя игрока");
        }

        return CompletionResult.Empty;
    }
}
