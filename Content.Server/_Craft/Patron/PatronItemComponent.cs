using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using System;

namespace Content.Server._Craft.Patron
{
    [RegisterComponent]
    [Access(typeof(PatronSystem))]
    public sealed class PatronItemComponent : Component
    {
        [DataField("enabled")]
        public bool Enabled = true;

        [DataField("patronOwner")]
        public Guid Patron = default!;
    }
}
