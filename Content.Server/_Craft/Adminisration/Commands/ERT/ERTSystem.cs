using Content.Server.Shuttles.Systems;
using Content.Server.Chat.Systems;
using Robust.Server.Player;
using Content.Shared.Mobs.Systems;
using Content.Shared.Zombies;
using Content.Server.GameTicking.Rules.Components;
using Robust.Shared.Map;
using System.Linq;
using Content.Server.Players;
using Content.Server.Traitor;
using Robust.Shared.Prototypes;
using Robust.Server.GameObjects;
using JetBrains.Annotations;
using Content.Server.Spawners.Components;
using Robust.Shared.Configuration;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using System.Collections.Generic;
using System;
using Content.Server._Craft.Utils;

namespace Content.Server._Craft.Administration.Commands.ERT;

[UsedImplicitly]
public sealed class ERTSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem ChatSystem = default!;
    [Dependency] private readonly ShuttleSystem ShuttleSystem = default!;
    [Dependency] private readonly IPlayerManager PlayerManager = default!;
    [Dependency] private readonly MobStateSystem MobState = default!;
    [Dependency] private readonly IMapManager MapManager = default!;
    [Dependency] private readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly MapLoaderSystem MapLoaderSystem = default!;
    [Dependency] private readonly IConfigurationManager Config = default!;

    private MapId MapId = MapId.Nullspace;
    private EntityUid ShuttleUid = EntityUid.Invalid;
    private ERTStatus ERTStatus = ERTStatus.IDLE;

    private bool ERTEnabled = false;
    public override void Initialize()
    {
        base.Initialize();

        Config.OnValueChanged(CCVars.ERTEnabled, ERTEnabledChanged, true);

        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);
        SetubByCVars();
    }

    private void ERTEnabledChanged(bool enabled)
    {
        ERTEnabled = enabled;
    }

    private void SetubByCVars()
    {
        ERTEnabled = Config.GetCVar(CCVars.ERTEnabled);
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        Cleanup();
    }

    /**
    Запомним, что при рестарте раунда, игра сама удаляет все сущности (в том числе карты)
    **/
    private void Cleanup()
    {
        ERTStatus = ERTStatus.IDLE;
    }

    public bool IsAvailable()
    {
        return ERTEnabled && ERTStatus == ERTStatus.IDLE;
    }

    public void CallERT()
    {
        if (ERTStatus != ERTStatus.IDLE)
            return;

        if (!AddShuttle())
        {
            ERTStatus = ERTStatus.ERROR;
            ERTReasonMessage(ERTReason.DECLINED_BY_ERROR);
            return;
        }

        if (CheckOperativesSpawns())
        {
            ERTStatus = ERTStatus.ERROR;
            ERTReasonMessage(ERTReason.DECLINED_BY_ERROR);
            return;
        }

        ERTReasonMessage(ERTReason.CONFIRMED_BY_AUTOMATIC);
    }

    private void ERTReasonMessage(ERTReason reason)
    {
        switch (reason)
        {
            case ERTReason.CONFIRMED_BY_ADMIN:
            case ERTReason.CONFIRMED_BY_AUTOMATIC:
                ERTStatus = ERTStatus.CALLED;
                SendMessage("ert-confirmed");
                break;

            case ERTReason.DECLINED_BY_AUTOMATIC:
            case ERTReason.DECLINED_BY_ADMIN:
                ERTStatus = ERTStatus.IDLE;
                SendMessage("ert-declined");
                break;

            case ERTReason.DECLINED_BY_ERROR:
                ERTStatus = ERTStatus.ERROR;
                SendMessage("ert-error");
                break;
        }
    }

    private void SendMessage(string locCode)
    {
        ChatUtils.SendLocMessageFromCentcom(
            chatSystem: ChatSystem,
            locCode: locCode,
            stationId: null
        );
    }

    private bool AddShuttle()
    {
        var shuttlePath = PrototypeManager.EnumeratePrototypes<ERTShuttlePrototype>()
                    ?.First()
                    ?.Path
                    .ToString();

        if (shuttlePath == null)
        {
            return false;
        }

        ShuttleUid = EntityUid.Invalid;
        MapId = MapId.Nullspace;

        (MapId, ShuttleUid) = ShuttleUtils.CreateShuttleOnNewMap(
            mapManager: MapManager,
            mapSystem: MapLoaderSystem,
            entityManager: EntityManager,
            shuttlePath: shuttlePath
        );

        return true;
    }

    private bool CheckOperativesSpawns()
    {
        var spawns = new List<TransformComponent>();
        foreach (var (spawnPoint, meta, xform) in EntityManager.EntityQuery<SpawnPointComponent, MetaDataComponent, TransformComponent>(true))
        {
            if (xform == null || xform.ParentUid != ShuttleUid || meta.EntityPrototype?.Parents?.Contains("RandomHumanoidSpawnerERTLeader") == false)
                continue;

            spawns.Add(xform);
        }

        return spawns.Count() > 0;
    }

    private bool IsPlayerTraitor(IPlayerSession session)
    {
        return session.ContentData()
                ?.Mind
                ?.HasRole<TraitorRole>() ?? false;
    }

    private bool IsPlayerStable(IPlayerSession session)
    {
        var attached = session.AttachedEntity;
        if (attached != null)
        {
            return MobState.IsAlive((EntityUid) attached) && !MobState.IsCritical((EntityUid) attached);
        }

        return false;
    }

    private IEnumerable<EntityUid?> GetStableAlivePlayers(EntityUid except)
    {
        foreach (var player in PlayerManager.Sessions)
        {
            if (player.AttachedEntity is { Valid: true } attached)
            {
                if (attached == except) continue;

                if (TryComp<ZombieComponent>(attached, out _) || TryComp<NukeOperativeComponent>(attached, out _))
                    continue;

                if (MobState.IsAlive(attached) && !MobState.IsCritical(attached))
                    yield return attached;
            }
        }
    }

    private enum ERTReason
    {
        CONFIRMED_BY_ADMIN,
        CONFIRMED_BY_AUTOMATIC,
        DECLINED_BY_AUTOMATIC,
        DECLINED_BY_ADMIN,
        DECLINED_BY_ERROR
    }
}
