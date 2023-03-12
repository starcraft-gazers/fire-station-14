using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Craft.ERT;

[Prototype("ERTShuttle")]
public sealed class ERTShuttlePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("path")]
    public ResourcePath Path = default!;
}
