using Content.Shared.Actions.ActionTypes;
using Robust.Shared.Utility;
using Robust.Shared.Audio;
using Content.Shared.SCP.ConcreteSlab;
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;
using System.Collections.Generic;
using System;
using Robust.Shared.Analyzers;

namespace Content.Server.Roles.SCP.SCP173;

[RegisterComponent]
[Access(typeof(SCP173System))]
[ComponentReference(typeof(SharedSCP173Component))]
public sealed class SCP173Component : SharedSCP173Component
{
    [DataField("eyeSightRange")]
    public float EyeSightRange = 8;

    [DataField("spooksSoundCollection", required: true)]
    public SoundSpecifier SpooksSound = default!;

    [DataField("scaresSoundCollection", required: true)]
    public SoundSpecifier ScaresSound = default!;

    [DataField("killSoundCollection", required: true)]
    public SoundSpecifier KillSound = default!;

    [DataField("doorOpenSound", required: true)]
    public SoundSpecifier DoorOpenSound = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    [DataField("lookers")]
    public List<EntityUid> Lookers = new();

    [DataField("shartAction")]
    public InstantAction ShartAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(30),
        Icon = new SpriteSpecifier.Texture(new("Interface/Actions/malfunction.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "scp-173-shart",
        Description = "scp-173-shart-desc",
        Priority = -1,
        Event = new ShartActionEvent(),
    };
    [DataField("blindAction")]
    public InstantAction BlindAction = new()
    {
        Enabled = false,
        UseDelay = TimeSpan.FromSeconds(90),
        Icon = new SpriteSpecifier.Texture(new("Interface/Actions/malfunction.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "scp-173-blind",
        Description = "scp-173-blind-desc",
        Priority = -1,
        Event = new BlindActionEvent(),
    };
    [DataField("doorOpenAction")]
    public InstantAction DoorOpenAction = new()
    {
        UseDelay = TimeSpan.FromSeconds(20),
        Icon = new SpriteSpecifier.Texture(new("Interface/Actions/malfunction.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "scp-173-dooropen",
        Description = "scp-173-dooropen-desc",
        Priority = -1,
        Event = new DoorOpenActionEvent(),
    };
}
