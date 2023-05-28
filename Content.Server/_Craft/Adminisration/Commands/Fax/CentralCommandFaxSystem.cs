using System.Linq;
using Content.Server._Craft.Utils;
using Content.Server.Chat.Systems;
using Content.Shared.GameTicking;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Craft.Administration.Commands.Fax;

[UsedImplicitly]
public sealed class CreateCentralCommandFaxSystem : EntitySystem
{
    [Dependency] private readonly IMapManager MapManager = default!;
    [Dependency] private readonly MapLoaderSystem MapLoader = default!;
    [Dependency] private readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    private static string MapPath = "/Maps/FireStation/centcomfax.yml";

    private EntityUid _gridWithFax = EntityUid.Invalid;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        var mapId = MapManager.CreateMap();
        if (!MapLoader.TryLoad(mapId, new ResPath(MapPath).ToString(), out var grids) || grids == null || grids.Count <= 0)
        {
            MapManager.DeleteMap(mapId);
            return;
        }

        _gridWithFax = grids[0];

        MapManager.SetMapPaused(mapId, false);
        ChatUtils.SendMessageFromCentcom(
            chatSystem: _chatSystem,
            message: "Соединение с факсом ЦК установлено",
            sender: "ИИ Помощник",
            null
        );
    }

    public EntityUid GetFaxArea()
    {
        return _gridWithFax;
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        _gridWithFax = EntityUid.Invalid;
    }
}
