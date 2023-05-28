using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;

namespace Content.Server.Xenos
{

    [RegisterComponent]
    public sealed class XenoQueenComponent : Component
    {
        [DataField("enabled")]
        public bool Enabled = true;

        [DataField("spawnPrototype", customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>))]
        public List<string> SpawnPrototypes = new List<string>()
        {
            "MobXenoDrone",
            "MobXenoSpitter",
            "MobXenoRunner"
        };

        [DataField("xenoBirthAction")]
        public InstantAction XenoBirthAction = new()
        {
            UseDelay = TimeSpan.FromSeconds(120),
            Icon = new SpriteSpecifier.Texture(new ResPath("Interface/Actions/malfunction.png")),
            ItemIconStyle = ItemActionIconStyle.NoItem,
            DisplayName = "xeno-queen-birth",
            Description = "xeno-queen-birth-desc",
            Priority = -1,
            Event = new XenoBirthActionEvent(),
        };

    }
}
