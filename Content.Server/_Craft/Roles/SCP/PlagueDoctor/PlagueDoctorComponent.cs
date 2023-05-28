using System;
using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Server.Roles.SCP.PlagueDoctor;

[RegisterComponent]
public sealed class PlagueDoctorComponent : Component
{

    [DataField("HealAction")]
    public EntityTargetAction HealAction = new()
    {
        Icon = new SpriteSpecifier.Rsi(new ResPath("Structures/Decoration/banner.rsi"), "banner_medical"),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "Вылечить",
        Description = "Полностью излечите вашу цель",
        Priority = -1,
        Event = new PlagueDoctorHealEvent(),
        CheckCanAccess = false,
        UseDelay = TimeSpan.FromSeconds(120f),
        Range = 6f
    };

    [DataField("MakeZombieAction")]
    public EntityTargetAction MakeZombieAction = new EntityTargetAction()
    {
        Icon = new SpriteSpecifier.Texture(new ResPath("Interface/Actions/zombie-turn.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "Сделать прислужника",
        Description = "Делает из цели прислужника, который является вашим спутником и подчиняется вам",
        Priority = -1,
        Event = new PlagueDoctorZombieEvent(),
        CheckCanAccess = false,
        UseDelay = TimeSpan.FromSeconds(600f),
        Range = 6f
    };

    [DataField("UnZombieAction")]
    public EntityTargetAction UnZombieAction = new EntityTargetAction()
    {
        Icon = new SpriteSpecifier.Rsi(new ResPath("Actions/Implants/implants.rsi"), "freedom"),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "Освободить прислужника",
        Description = "Особождает цель от служения вам",
        Priority = -1,
        Event = new PlagueDoctorUnZombieEvent(),
        CheckCanAccess = false,
        UseDelay = TimeSpan.FromSeconds(10),
        Range = 6f
    };

    [DataField("OpenDoorAction")]
    public EntityTargetAction OpenDoorAction = new EntityTargetAction()
    {
        Icon = new SpriteSpecifier.Texture(new("Interface/Actions/malfunction.png")),
        ItemIconStyle = ItemActionIconStyle.NoItem,
        DisplayName = "Открыть дверь",
        Description = "Открывает запертую дверь, к которой у вас нет доступа",
        Priority = -1,
        Event = new PlagueDoctorOpenDoorEvent(),
        UseDelay = TimeSpan.FromSeconds(60),
        Range = 6f
    };
}
