using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Serialization.Manager.Attributes;

[RegisterComponent]
public sealed class PlagueDoctorZombieComponent : Component
{
    [DataField("skinColor")]
    public Color SkinColor = new(1.48f, 0.09f, 0.37f);

    [DataField("eyeColor")]
    public Color EyeColor = new(0.96f, 0.13f, 0.24f);

    [DataField("oldSkinColor")]
    public Color OldSkinColor = Color.Aqua;

    [DataField("oldEyeColor")]
    public Color OldEyeColor = Color.Aqua;

    [DataField("baseLayerExternal")]
    public string BaseLayerExternal = "MobHumanoidMarkingMatchSkin";

    [DataField("OldName")]
    public string OldName = string.Empty;

    public EntityUid ZombieOwner = EntityUid.Invalid;
}
