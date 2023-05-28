using Content.Server.Roles.SCP.Science;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Shared.Doors;
using Robust.Server.Player;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Random;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Content.Server._Craft.SCP
{
    public sealed class SCPGeneralSystem : EntitySystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly GameTicker _ticker = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly SCPStationSystem _scpStationSystem = default!;
        private List<IPlayerSession> scpPlayers = new List<IPlayerSession>();
        private Dictionary<string, List<EntityUid>> _containmentDoors = new();
        // TODO: Заменить этот ужас на прототип
        private static string[] _friendlyScps = {
            "MobSCPSoap",
            "MobSCPPlagueDoctor"
        };
        private static string[] _hostileScps = {
            "MobSCP173"
        };

        public bool IsSCPBreakoutActive { get; private set; }

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayersSpawning);

            SubscribeLocalEvent<ScpContainmentDoorComponent, ComponentInit>(OnContainmentComponentInit);
            SubscribeLocalEvent<ScpContainmentDoorComponent, ComponentRemove>(OnContainmentComponentRemove);
            SubscribeLocalEvent<ScpContainmentDoorComponent, BeforeDoorClosedEvent>(OnDoorCloseTryRemove);
        }

        private void OnPlayersSpawning(RulePlayerSpawningEvent ev)
        {
            if(!_scpStationSystem.ShouldSpawnSCP())
                return;

            var everyone = ev.PlayerPool;

            var safePrefList = new List<IPlayerSession>();
            var safeSCPList = new List<EntityUid>();

            var hostPrefList = new List<IPlayerSession>();
            var hostSCPList = new List<EntityUid>();

            foreach (var (meta, xform) in EntityManager.EntityQuery<MetaDataComponent, TransformComponent>(true))
            {
                var proto = meta.EntityPrototype;
                var parents = proto?.Parents;
                if (proto is null || parents is null)
                    continue;

                if (!parents.Contains("SCPMobBase"))
                    continue;

                var id = proto.ID;
                var ent = xform.Owner;

                if (_friendlyScps.Contains(id))
                {
                    safeSCPList.Add(ent);
                }
                else if (_hostileScps.Contains(id))
                {
                    hostSCPList.Add(ent);
                }
            }

            foreach (var player in everyone)
            {
                if (!ev.Profiles.ContainsKey(player.UserId))
                {
                    continue;
                }
                var profile = ev.Profiles[player.UserId];
                if (profile.AntagPreferences.Contains("SafeSCP"))
                {
                    safePrefList.Add(player);
                }
                if (profile.AntagPreferences.Contains("HostileSCP"))
                {
                    hostPrefList.Add(player);
                }
            }

            SpawnAsSCPs(hostPrefList, hostSCPList, ev.PlayerPool);
            SpawnAsSCPs(safePrefList, safeSCPList, ev.PlayerPool);
        }

        private void SpawnAsSCPs(List<IPlayerSession> plys, List<EntityUid> scps, List<IPlayerSession> pool)
        {
            var maxScps = scps.Count;
            if (maxScps == 0)
                return;

            _random.Shuffle(plys);

            foreach (var session in plys)
            {
                if (!pool.Contains(session))
                    continue;

                var trgscp = scps[0];
                var newMind = new Mind.Mind(session.UserId)
                {
                    CharacterName = MetaData(trgscp).EntityName
                };

                newMind.ChangeOwningPlayer(session.UserId);
                newMind.TransferTo(trgscp);

                scps.RemoveAt(0);
                pool.Remove(session);

                scpPlayers.Add(session);
                _ticker.PlayerJoinGame(session);
                if (scps.Count == 0)
                    break;
            }
        }

        private void OnContainmentComponentInit(EntityUid uid, ScpContainmentDoorComponent comp, ComponentInit args)
        {
            var group = comp.DoorGroup;
            List<EntityUid> groupList;
            if (_containmentDoors.ContainsKey(group))
                groupList = _containmentDoors[group];
            else
            {
                groupList = new();
                _containmentDoors[group] = groupList;
            }
            groupList.Add(uid);
        }
        private void OnContainmentComponentRemove(EntityUid uid, ScpContainmentDoorComponent comp, ComponentRemove args)
        {
            var group = comp.DoorGroup;
            if (!_containmentDoors.ContainsKey(group))
                return;
            var groupList = _containmentDoors[group];
            groupList.Remove(uid);
            if (groupList.Count == 0)
                _containmentDoors.Remove(group);
        }
        private void OnDoorCloseTryRemove(EntityUid uid, ScpContainmentDoorComponent comp, BeforeDoorClosedEvent args)
        {
            if (comp.DoorBlocked)
                args.Cancel();
        }

        public List<EntityUid> GetGroupedDoors(string group)
        {
            return _containmentDoors[group];
        }
        public List<EntityUid>? GetGroupedDoorsOrNull(string group)
        {
            if (_containmentDoors.TryGetValue(group, out var groupList))
                return groupList;
            return null;
        }
        public List<string> GetGroupedDoorsGroups()
        {
            var res = new List<string>();
            foreach (var pair in _containmentDoors)
                res.Add(pair.Key);
            return res;
        }
    }
}
