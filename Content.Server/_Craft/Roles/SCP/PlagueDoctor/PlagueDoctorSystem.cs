using System.Linq;
using Content.Server.Humanoid;
using Content.Server.Nutrition.Components;
using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Shared.Actions;
using Content.Shared.Humanoid;
using Content.Shared.Rejuvenate;
using Content.Server.Chat.Managers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Server.GameObjects;
using Content.Shared.Doors.Components;
using Content.Server.Doors.Systems;

namespace Content.Server.Roles.SCP.PlagueDoctor;

public sealed class PlagueDoctorSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _sharedHuApp = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly DoorSystem _doorSys = default!;

    public override void Initialize()
    {
        base.Initialize();


        SubscribeLocalEvent<PlagueDoctorComponent, ComponentInit>(OnPlagueDoctorInit);
        SubscribeLocalEvent<PlagueDoctorComponent, ComponentRemove>(OnPlagueDoctorRemove);
        SubscribeLocalEvent<PlagueDoctorComponent, PlagueDoctorHealEvent>(OnHealEvent);
        SubscribeLocalEvent<PlagueDoctorComponent, PlagueDoctorZombieEvent>(OnMakeZombieEvent);
        SubscribeLocalEvent<PlagueDoctorComponent, PlagueDoctorUnZombieEvent>(OnUnZombieEvent);
        SubscribeLocalEvent<PlagueDoctorComponent, PlagueDoctorOpenDoorEvent>(OnOpenDoorEvent);
    }

    private void OnPlagueDoctorInit(EntityUid uid, PlagueDoctorComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, component.HealAction, uid);
        _actionsSystem.AddAction(uid, component.MakeZombieAction, uid);
        _actionsSystem.AddAction(uid, component.UnZombieAction, uid);
        _actionsSystem.AddAction(uid, component.OpenDoorAction, uid);
    }

    private void OnPlagueDoctorRemove(EntityUid uid, PlagueDoctorComponent component, ComponentRemove args)
    {
        _actionsSystem.RemoveAction(uid, component.HealAction);
        _actionsSystem.RemoveAction(uid, component.MakeZombieAction);
        _actionsSystem.RemoveAction(uid, component.UnZombieAction);
        _actionsSystem.RemoveAction(uid, component.OpenDoorAction);
    }

    private void OnHealEvent(EntityUid uid, PlagueDoctorComponent component, PlagueDoctorHealEvent args)
    {
        args.Handled = true;
        var target = args.Target;

        if (HasComp<HumanoidAppearanceComponent>(target) || target == uid)
        {
            RaiseLocalEvent(target, new RejuvenateEvent());
            _popupSystem.PopupEntity("Чумной доктор исцелил вас!", target);
        }
    }

    private void OnMakeZombieEvent(EntityUid uid, PlagueDoctorComponent component, PlagueDoctorZombieEvent args)
    {
        args.Handled = true;
        var target = args.Target;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popupSystem.PopupEntity("Мы можете превратить в своих прислужников только людей", uid);
            return;
        }

        var zombies = EntityQuery<PlagueDoctorZombieComponent>()?.Where(p => p.ZombieOwner == uid);
        if (zombies != null && zombies.Count() == 2)
        {
            _popupSystem.PopupEntity("Вы не можете иметь больше двух прислужников", uid);
            return;
        }

        RemComp<ThirstComponent>(target);

        EnsureComp<ReplacementAccentComponent>(target).Accent = "mute";

        var zombiecomp = EnsureComp<PlagueDoctorZombieComponent>(target);
        zombiecomp.ZombieOwner = uid;

        if (TryComp<HumanoidAppearanceComponent>(target, out var huApComp))
        {
            zombiecomp.OldEyeColor = huApComp.EyeColor;
            zombiecomp.OldSkinColor = huApComp.SkinColor;

            _sharedHuApp.SetSkinColor(target, zombiecomp.SkinColor, humanoid: huApComp);
            _sharedHuApp.SetBaseLayerColor(target, HumanoidVisualLayers.Eyes, zombiecomp.EyeColor, humanoid: huApComp);

            _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.Tail, zombiecomp.BaseLayerExternal, humanoid: huApComp);
            _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.HeadSide, zombiecomp.BaseLayerExternal, humanoid: huApComp);
            _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.HeadTop, zombiecomp.BaseLayerExternal, humanoid: huApComp);
            _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.Snout, zombiecomp.BaseLayerExternal, humanoid: huApComp);
        }

        var meta = MetaData(target);
        zombiecomp.OldName = meta.EntityName;
        meta.EntityName = $"Спутник чумного доктора {meta.EntityName}";

        _popupSystem.PopupEntity("Чумной доктор превратил вас в своего прислужника... Теперь вы подчиняетесь ему", target);

        if (!TryComp<ActorComponent>(target, out var actor))
            return;

        _chatManager.DispatchServerMessage(
            player: actor.PlayerSession,
             message: "Чумной Доктор превратил вас в своего прислужника!\nДелайте все, что он говорит, и не отходите от него (если он не попросит обратного).\nНе атакуйте других игроков преждевременно, если нет угрозы жизни Чумного Доктора"
        );
    }

    private void OnUnZombieEvent(EntityUid uid, PlagueDoctorComponent component, PlagueDoctorUnZombieEvent args)
    {
        args.Handled = true;
        var target = args.Target;

        if (!TryComp<PlagueDoctorZombieComponent>(target, out var zombiecomp) || zombiecomp.ZombieOwner != uid)
        {
            _popupSystem.PopupEntity("Он не ваш прислужник", uid);
            return;
        }

        EnsureComp<ThirstComponent>(target);

        if (!TryComp<HumanoidAppearanceComponent>(target, out var targetHumanoidComp))
        {
            RemComp<PlagueDoctorZombieComponent>(target);
            RemComp<ReplacementAccentComponent>(target);
            _popupSystem.PopupEntity("Вы освободили его душу, но цвет кожи не изменился", uid);
            SendTextToChat(target, "Чумной доктор избавил вас от мучений... Вы больше не его прислужник");
            return;
        }

        _sharedHuApp.SetSkinColor(target, zombiecomp.OldSkinColor, humanoid: targetHumanoidComp);
        _sharedHuApp.SetBaseLayerColor(target, HumanoidVisualLayers.Eyes, zombiecomp.OldEyeColor, humanoid: targetHumanoidComp);

        _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.Tail, zombiecomp.BaseLayerExternal, humanoid: targetHumanoidComp);
        _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.HeadSide, zombiecomp.BaseLayerExternal, humanoid: targetHumanoidComp);
        _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.HeadTop, zombiecomp.BaseLayerExternal, humanoid: targetHumanoidComp);
        _sharedHuApp.SetBaseLayerId(target, HumanoidVisualLayers.Snout, zombiecomp.BaseLayerExternal, humanoid: targetHumanoidComp);

        var meta = MetaData(target);
        meta.EntityName = zombiecomp.OldName;

        RemComp<ReplacementAccentComponent>(target);
        RemComp<PlagueDoctorZombieComponent>(target);

        _popupSystem.PopupEntity("Вы освободили его душу.. Он больше не ваш прислужник", uid);
        SendTextToChat(target, "Чумной доктор избавил вас от мучений... Вы больше не его прислужник");
    }

    private void OnOpenDoorEvent(EntityUid uid, PlagueDoctorComponent component, PlagueDoctorOpenDoorEvent args)
    {
        args.Handled = true;
        var target = args.Target;

        if (TryComp<DoorComponent>(target, out var doorComp) && doorComp.ClickOpen && doorComp.State == DoorState.Closed)
        {
            _doorSys.TryOpen(target, doorComp, quiet: false);
        }
    }

    private void SendTextToChat(EntityUid target, string message)
    {
        if (!TryComp<ActorComponent>(target, out var actor))
            return;

        _chatManager.DispatchServerMessage(
            player: actor.PlayerSession,
            message: message
        );
    }
}
