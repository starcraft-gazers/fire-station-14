using System;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server._Craft.Administration.Commands.ERT;

[Serializable, Prototype("ERTShuttle")]
public sealed class ERTShuttlePrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("path")]
    public ResPath Path = default!;
}
