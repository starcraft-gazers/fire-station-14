using Content.Shared.SCP.ConcreteSlab;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;

namespace Content.Client._Craft.SCP
{
    [RegisterComponent]
    [Access(typeof(SCP173System))]
    [ComponentReference(typeof(SharedSCP173Component))]
    public sealed class SCP173Component : SharedSCP173Component { }
}
