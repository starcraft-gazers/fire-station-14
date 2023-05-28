using System;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Server.Roles.Priest;

[RegisterComponent]
public sealed class PriestHealDiseaseComponent : Component
{
    [DataField("HealDiseaseAction")]
    public InstantAction HealDiseaseAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(180),
        Icon = new SpriteSpecifier.Texture(new ResPath("Structures/Decoration/banner.rsi/banner_medical.png")),
        DisplayName = "Молитва за здравие",
        Description = "Помолитесь за здравие, чтобы попытаться излечить людей вокруг от болезней",
        Priority = -1,
        Event = new HealDiseaseActionEvent(),
    };
}
