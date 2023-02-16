using Content.Server.Explosion.EntitySystems;
using Content.Server.Ninja.Components;
using Content.Server.Sticky.Events;
using Content.Server.Popups;
using Content.Shared.Interaction;

namespace Content.Server.Ninja.Systems;

public sealed class SpiderChargeSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popups = default!;

    public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<SpiderChargeComponent, BeforeRangedInteractEvent>(BeforePlant);
        SubscribeLocalEvent<SpiderChargeComponent, EntityStuckEvent>(OnStuck);
        SubscribeLocalEvent<SpiderChargeComponent, TriggerEvent>(OnExplode);
    }

	private void BeforePlant(EntityUid uid, SpiderChargeComponent comp, BeforeRangedInteractEvent args)
	{
		var user = args.User;

		if (!TryComp<SpaceNinjaComponent>(user, out var ninja))
		{
			_popups.PopupEntity(Loc.GetString("spider-charge-not-ninja"), user, user);
			args.Handled = true;
			return;
		}

		// allow planting anywhere if there is no target, which should never happen
		if (ninja.SpiderChargeTarget != null)
		{
			// assumes warp point still exists
			var target = Transform(ninja.SpiderChargeTarget.Value).MapPosition;
			if (!args.ClickLocation.ToMap(EntityManager).InRange(target, comp.Range))
			{
				_popups.PopupEntity(Loc.GetString("spider-charge-too-far"), user, user);
				args.Handled = true;
				return;
			}
		}
	}

    private void OnStuck(EntityUid uid, SpiderChargeComponent comp, EntityStuckEvent args)
    {
        comp.Planter = args.User;
    }

    private void OnExplode(EntityUid uid, SpiderChargeComponent comp, TriggerEvent args)
    {
        if (comp.Planter == null || !TryComp<SpaceNinjaComponent>(comp.Planter, out var ninja))
            return;

        // assumes the target was destroyed, that the charge wasn't moved somehow
        ninja.SpiderChargeDetonated = true;
    }
}
