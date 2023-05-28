using Content.Server.Visible;
using Content.Shared.Actions;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using Content.Server.Popups;

namespace Content.Server.Roles.Priest;

public sealed class PriestPowersSystem : EntitySystem
{

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookupSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly VisibilitySystem _visibilitySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PriestSeeGhostComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PriestSeeGhostComponent, SeeGhostActionEvent>(OnSeeGhost);

        SubscribeLocalEvent<PriestHealDiseaseComponent, ComponentInit>(OnHealDesearComponentInit);
        SubscribeLocalEvent<PriestHealDiseaseComponent, HealDiseaseActionEvent>(OnHealDesearComponentInit);
    }

    private void OnHealDesearComponentInit(EntityUid uid, PriestHealDiseaseComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, component.HealDiseaseAction, uid);
    }

    private void OnHealDesearComponentInit(EntityUid uid, PriestHealDiseaseComponent component, HealDiseaseActionEvent args)
    {
        args.Handled = true;

        if (_random.Next(1, 100) < 60)
        {
            _popupSystem.PopupEntity("Кажется ваши молитвы не были услышаны", uid);
            return;
        }

        var entitiesInRange = _entityLookupSystem.GetEntitiesInRange(uid, 3);
        if (entitiesInRange == null)
            return;

        foreach (var entity in entitiesInRange)
        {
            // if (!EntityManager.TryGetComponent<DiseaseCarrierComponent?>(entity, out var diseaseCarrierComponent) || diseaseCarrierComponent == null)
            //     continue;

            // _diseaseSystem.CureAllDiseases(entity, diseaseCarrierComponent);
        }

        _popupSystem.PopupEntity("Бог услышал вас!", uid);
    }

    private void OnComponentInit(EntityUid uid, PriestSeeGhostComponent component, ComponentInit args)
    {
        // var visibility = EntityManager.EnsureComponent<VisibilityComponent>(uid);
        // EntityManager.TryGetComponent<MetaDataComponent>(uid, out var metadata);

        // _visibilitySystem.AddLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
        // _visibilitySystem.AddLayer(uid, visibility, (int) VisibilityFlags.Ghost, false);

        // _visibilitySystem.RefreshVisibility(uid, metadata, visibility);

        _actionsSystem.AddAction(uid, component.SeeGhostAction, uid);
    }

    private void OnSeeGhost(EntityUid uid, PriestSeeGhostComponent component, SeeGhostActionEvent args)
    {
        args.Handled = true;

        var visibility = EntityManager.EnsureComponent<VisibilityComponent>(uid);

        _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Ghost, false);
        _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Normal, false);
        _visibilitySystem.RefreshVisibility(visibility);

        if (!EntityManager.TryGetComponent(uid, out EyeComponent? eye) || eye == null)
        {
            _popupSystem.PopupEntity("Бог не дает вам этой силы", uid);
            return;
        }

        eye.VisibilityMask ^= (uint) VisibilityFlags.Ghost;
        component.StartTime = _timing.CurTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var seeGhostComponent in EntityQuery<PriestSeeGhostComponent>())
        {
            var curTime = _timing.CurTime;
            if (seeGhostComponent.StartTime != System.TimeSpan.Zero && curTime - seeGhostComponent.StartTime >= seeGhostComponent.DelayEndTime)
            {
                if (EntityManager.TryGetComponent(seeGhostComponent.Owner, out VisibilityComponent? visibility))
                {
                    _visibilitySystem.RemoveLayer(visibility, (int) VisibilityFlags.Ghost, false);
                    _visibilitySystem.AddLayer(visibility, (int) VisibilityFlags.Normal, false);
                    _visibilitySystem.RefreshVisibility(visibility);
                }

                if (EntityManager.TryGetComponent(seeGhostComponent.Owner, out EyeComponent? eye))
                {
                    eye.VisibilityMask ^= (uint) VisibilityFlags.Ghost;
                }

                seeGhostComponent.StartTime = System.TimeSpan.Zero;
            }
        }
    }
}

public sealed class SeeGhostActionEvent : InstantActionEvent { }
public sealed class HealDiseaseActionEvent : InstantActionEvent { }
