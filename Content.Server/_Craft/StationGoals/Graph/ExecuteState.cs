namespace Content.Server._Craft.StationGoals.Graph;

internal enum ExecuteState
{
    Idle,
    InProgress,
    WaitingDelay,
    Finished,
    Interrupted,
    InnerInterrupted
}
