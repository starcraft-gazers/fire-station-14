using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;
using Robust.Shared.Audio;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using System;

namespace Content.Server.Roles.SCP.SOAP;

[RegisterComponent]
public sealed class SCPSoapComponent : Component
{
    [DataField("slipActionRange")]
    public float SlipActionRange = 1;

    [DataField("slipActionForce")]
    public float SlipActionForce = 15;

    [DataField("slipActionStun")]
    public float SlipActionStun = 4;

    [DataField("slipActionSound")]
    public SoundSpecifier SlipActionSound = new SoundPathSpecifier("/Audio/Effects/slip.ogg");

    [DataField("slipAction")]
    public InstantAction SlipAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(32),
        Icon = new SpriteSpecifier.Texture(new("Interface/Actions/malfunction.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "Подножка",
        Description = "Вы отправляете всех кто ходит рядом с вами в путешествие лицом к полу!",
        Priority = -1,
        Event = new SlipActionEvent(),
    };
}
