using System.Collections.Generic;
using Content.Server._Craft.Utils;
using Content.Server.Chat.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server._Craft.StationGoals.Graph.Steps.Announcements;

public sealed class AnnouncementStep : Step
{
    [DataField("sender", serverOnly: true, required: true)]
    private string Sender = default!;

    [DataField("messageLoc", serverOnly: true, required: true)]
    private string MessageLoc = default!;

    internal override ExecuteState ExecuteStep(Dictionary<StepDataKey, object> results, StationGoalPaperSystem system)
    {
        var chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();

        var message = Loc.GetString(MessageLoc);

        ChatUtils.SendLocMessageFromCustom(
            chatSystem: chatSystem,
            locCode: message,
            sender: Sender,
            stationId: null
        );

        system.logger.RootSawmill.Debug($"Step: {Name} finished success");
        return ExecuteState.Finished;
    }
}
