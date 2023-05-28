using System;
using Content.Server.Maps.NameGenerators;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Roles.SCP.Science;

[Serializable, Prototype("SCPStationPrototype")]
public sealed class SCPStationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField("mapPath", required: true, serverOnly: true)]
    public string MapPath = default!;

    [DataField("shuttlePath", required: true, serverOnly: true)]
    public string ShuttlePath = default!;

    [DataField("stationNameTemplate", required: true, serverOnly: true)]
    public string StationNameTemplate = default!;

    [DataField("nameGenerator", required: true, serverOnly: true)]
    public StationNameGenerator? NameGenerator { get; }
}
