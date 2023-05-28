using System;
using System.Collections.Generic;
using Content.Server._Craft.StationGoals.Graph;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._Craft.StationGoals;

[Serializable, Prototype("stationGoal")]
public sealed class StationGoalPrototype : IPrototype
{
    [IdDataFieldAttribute]
    public string ID { get; } = default!;

    [DataField("canStartAutomatic", serverOnly: true)]
    public readonly bool CanStartAutomatic = true;

    [DataField("graph", serverOnly: true)]
    public readonly StationGoalGraph[] _graphs = default!;

    public int CurrentGraphIndex = 0;

    public void Cleanup()
    {
        CurrentGraphIndex = 0;
        foreach (var graph in _graphs)
        {
            graph.Cleanup();
        }
    }
}

