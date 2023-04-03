using Content.Server.Chat.Managers;
using Content.Server.Mind.Commands;
using Content.Server.Mind.Components;
using Content.Server.Roles;
using Robust.Server.Player;

namespace Content.Server.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take over the Owner entity.
    /// </summary>
    [RegisterComponent, ComponentReference(typeof(GhostRoleComponent))]
    public sealed class GhostTakeoverAvailableComponent : GhostRoleComponent
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("blockedByAntagManager")]
        public bool BlockedByAntagManager = false;

        public override bool Take(IPlayerSession session)
        {
            if (Taken)
                return false;

            if (BlockedByAntagManager && !IsPlayerTimeValid(session))
            {
                IoCManager.Resolve<IChatManager>().DispatchServerMessage(
                    player: session,
                    message: "Вы не наиграли достаточное количество часов, для этой роли"
                );
                return false;
            }

            Taken = true;

            var mind = Owner.EnsureComponent<MindComponent>();

            if (mind.HasMind)
                return false;

            if (MakeSentient)
                MakeSentientCommand.MakeSentient(Owner, IoCManager.Resolve<IEntityManager>(), AllowMovement, AllowSpeech);

            var ghostRoleSystem = EntitySystem.Get<GhostRoleSystem>();
            ghostRoleSystem.GhostRoleInternalCreateMindAndTransfer(session, Owner, Owner, this);

            ghostRoleSystem.UnregisterGhostRole(this);

            return true;
        }

        private bool IsPlayerTimeValid(IPlayerSession player)
        {
            var antagManager = IoCManager.Resolve<IAntagManager>();
            return antagManager.IsPlayerTimeValidForGhostRole(player);
        }
    }
}
