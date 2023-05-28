using System;
using System.Collections.Generic;
using Content.Server._Craft.StationGoals.Graph.Steps;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph;

[ImplicitDataDefinitionForInheritors]
public abstract class Step
{
    [DataField("name", serverOnly: true)]
    private readonly string _name = string.Empty;

    [DataField("delay", serverOnly: true)]
    public readonly int Delay = 0;

    public string Name
    {
        get
        {
            if (String.IsNullOrEmpty(_name))
            {
                return this.GetType().Name;
            }

            return _name;
        }
    }

    internal Dictionary<StepDataKey, object> results = new Dictionary<StepDataKey, object>();
    internal ExecuteState State = ExecuteState.Idle;

    internal void Execute(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        if (!CanStartStep())
        {
            system.logger.RootSawmill.Debug($"Step: {Name} cant start cause of state {State}");
            return;
        }

        if (IsDelayRequired())
        {
            State = ExecuteState.WaitingDelay;
            system.AskForDelay(Delay);
            system.logger.RootSawmill.Debug($"Step: {Name} waiting for delay");
            return;
        }

        State = ExecuteStep(results, system);
    }

    private bool CanStartStep()
    {
        return State != ExecuteState.Interrupted && State != ExecuteState.Finished;
    }

    private bool IsDelayRequired()
    {
        return State != ExecuteState.WaitingDelay && Delay != 0;
    }

    internal abstract ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system);
    public virtual void Cleanup() { }
}
