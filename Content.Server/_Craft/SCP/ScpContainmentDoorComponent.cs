using System;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;

namespace Content.Server._Craft.SCP
{
    [RegisterComponent]
    public sealed class ScpContainmentDoorComponent : Component
    {
        [DataField("doorGroup")]
        [ViewVariables(VVAccess.ReadWrite)]
        public string DoorGroup = String.Empty;

        [DataField("doorBlocked")]
        [ViewVariables(VVAccess.ReadOnly)]
        public bool DoorBlocked = false;
    }
}
