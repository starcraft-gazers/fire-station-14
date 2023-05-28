using System;
using System.Linq;
using Content.Server._Craft.StationGoals.Graph;
using Content.Server._Craft.Utils;
using Content.Shared.GameTicking;
using Content.Shared.Random.Helpers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Craft.StationGoals;
public sealed class StationGoalPaperSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] internal readonly ILogManager logger = default!;

    private TimeSpan NextConditionCheck = TimeSpan.Zero;

    internal StationGoalPrototype? currentGoal = null;
    internal const int DEFAULT_DELAY_FOR_CONDITIONS = 30;

    public void AskForDelay(int seconds)
    {
        NextConditionCheck = _gameTiming.CurTime + new TimeSpan(hours: 0, minutes: 0, seconds: seconds);
    }
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundStartedEvent>(OnRoundStarted);
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);
    }

    private void OnRoundStarted(RoundStartedEvent ev)
    {
        CleanupGoal();
        SendRandomGoal();
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        CleanupGoal();
    }

    public void SendRandomGoal()
    {
        var availableGoals = _prototypeManager.EnumeratePrototypes<StationGoalPrototype>()
            .Where(prototype => prototype.CanStartAutomatic)
            .ToList();

        var goal = _random.Pick(availableGoals);
        SendStationGoal(goal);
    }


    public void SendStationGoal(StationGoalPrototype goal)
    {
        CleanupGoal();

        currentGoal = goal;
        ExecuteGraphs();
    }

    private void CleanupGoal()
    {
        if (currentGoal == null)
            return;

        currentGoal.Cleanup();
        currentGoal = null;
    }

    private void ExecuteGraphs()
    {
        if (currentGoal == null)
            return;

        foreach (var (graph, index) in currentGoal._graphs.WithIndex())
        {
            if (index < currentGoal.CurrentGraphIndex)
                continue;

            currentGoal.CurrentGraphIndex = index;

            if (!graph.Execute(this) && graph.State == ExecuteState.InnerInterrupted)
            {
                break;
            }
        }
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (currentGoal == null || NextConditionCheck == TimeSpan.Zero)
            return;

        if (NextConditionCheck >= _gameTiming.CurTime)
            return;

        NextConditionCheck = TimeSpan.Zero;
        ExecuteGraphs();
    }
}
