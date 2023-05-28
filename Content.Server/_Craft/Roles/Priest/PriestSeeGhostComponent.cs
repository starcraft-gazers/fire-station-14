using System;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server.Roles.Priest;

[RegisterComponent]
public sealed class PriestSeeGhostComponent : Component
{

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StartTime = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DelayEndTime = TimeSpan.FromSeconds(10);

    [DataField("SeeGhostAction")]
    public InstantAction SeeGhostAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(10),
        Icon = new SpriteSpecifier.Texture(new ResPath("Effects/crayondecals.rsi/ghost.png")),
        DisplayName = "Проникновение в загробный мир",
        Description = "Часть вашей души попадет в загробный мир",
        Priority = -1,
        Event = new SeeGhostActionEvent(),
    };
}
