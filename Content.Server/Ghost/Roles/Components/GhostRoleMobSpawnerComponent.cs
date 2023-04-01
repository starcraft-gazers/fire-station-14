using Content.Server.Mind.Commands;
using Content.Server.Mind.Components;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Server.Ghost.Roles.Events;
using Content.Server.Roles;
using Content.Server.Chat.Managers;

namespace Content.Server.Ghost.Roles.Components
{
    /// <summary>
    ///     Allows a ghost to take this role, spawning a new entity.
    /// </summary>
    [RegisterComponent, ComponentReference(typeof(GhostRoleComponent))]
    public sealed class GhostRoleMobSpawnerComponent : GhostRoleComponent
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("deleteOnSpawn")]
        private bool _deleteOnSpawn = true;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("availableTakeovers")]
        private int _availableTakeovers = 1;

        [ViewVariables]
        private int _currentTakeovers = 0;

        [CanBeNull]
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("prototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? Prototype { get; private set; }

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("blockedByAntagManager")]
        public bool BlockedByAntagManager = false;

        public override bool Take(IPlayerSession session)
        {
            if (Taken)
                return false;

            if (string.IsNullOrEmpty(Prototype))
                throw new NullReferenceException("Prototype string cannot be null or empty!");

            if (BlockedByAntagManager && !IsPlayerTimeValid(session))
            {
                IoCManager.Resolve<IChatManager>().DispatchServerMessage(
                    player: session,
                    message: $"Вы не наиграли достаточное количество часов, для этой роли"
                );
                return false;
            }

            var mob = _entMan.SpawnEntity(Prototype, _entMan.GetComponent<TransformComponent>(Owner).Coordinates);
            var xform = _entMan.GetComponent<TransformComponent>(mob);
            xform.AttachToGridOrMap();

            var spawnedEvent = new GhostRoleSpawnerUsedEvent(Owner, mob);
            _entMan.EventBus.RaiseLocalEvent(mob, spawnedEvent, false);

            if (MakeSentient)
                MakeSentientCommand.MakeSentient(mob, _entMan, AllowMovement, AllowSpeech);

            mob.EnsureComponent<MindComponent>();

            var ghostRoleSystem = EntitySystem.Get<GhostRoleSystem>();
            ghostRoleSystem.GhostRoleInternalCreateMindAndTransfer(session, Owner, mob, this);

            if (++_currentTakeovers < _availableTakeovers)
                return true;

            Taken = true;

            if (_deleteOnSpawn)
                _entMan.QueueDeleteEntity(Owner);

            return true;
        }

        private bool IsPlayerTimeValid(IPlayerSession player)
        {
            var antagManager = IoCManager.Resolve<IAntagManager>();
            return antagManager.IsPlayerTimeValidForGhostRole(player);
        }
    }
}
