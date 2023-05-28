using System.Collections.Generic;
using Content.Server.Fax;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Steps.Announcements;

public sealed class PrintGoalToFaxStep : Step
{
    [DataField("messageLoc", serverOnly: true, required: true)]
    private string MessageLoc = default!;
    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var goal = system.currentGoal;
        if (goal == null)
        {
            system.logger.RootSawmill.Debug($"Step: {Name} interruped goal is null");
            return ExecuteState.Interrupted;
        }

        var entityManager = IoCManager.Resolve<IEntityManager>();
        var entitySystemManager = IoCManager.Resolve<IEntitySystemManager>();
        var faxSystem = entitySystemManager.GetEntitySystem<FaxSystem>();
        var faxes = entityManager.EntityQuery<FaxMachineComponent>();

        foreach (var fax in faxes)
        {
            if (!fax.ReceiveStationGoal)
                continue;

            var printout = new FaxPrintout(
                Loc.GetString(MessageLoc),
                Loc.GetString("station-goal-fax-paper-name"),
                null,
                "paper_stamp-cent",
                new() { Loc.GetString("stamp-component-stamped-name-centcom") });
            faxSystem.Receive(fax.Owner, printout, null, fax);
        }

        system.logger.RootSawmill.Debug($"Step: {Name} finished success");
        return ExecuteState.Finished;
    }
}
