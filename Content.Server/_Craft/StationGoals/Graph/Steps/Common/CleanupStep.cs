using System.Collections.Generic;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Steps.Common;

public sealed class CleanupStep : Step
{
    [DataField("step", serverOnly: true, required: true)]
    private string StepName = default!;
    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var goal = system.currentGoal;
        if (goal == null){
            system.logger.RootSawmill.Debug($"Step: {Name} interrupted goal is null");
            return ExecuteState.Interrupted;
        }

        var currentGraph = goal._graphs[goal.CurrentGraphIndex];
        var targetStep = currentGraph.TryGetStepByName(StepName);

        targetStep?.Cleanup();

        system.logger.RootSawmill.Debug($"Step: {Name} finished success");
        return ExecuteState.Finished;
    }
}
