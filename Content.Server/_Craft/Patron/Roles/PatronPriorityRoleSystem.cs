using System;
using System.Collections.Generic;
using Content.Shared.GameTicking;
using Robust.Server.Player;
using Robust.Shared.GameObjects;

namespace Content.Server._Craft.Patron.Roles;

//Это временная затычка, потому что патроны уже бунт подымают и надо сделать
public sealed class PatronPriorityRoleSystem : EntitySystem
{
    private Dictionary<IPlayerSession, PatronRoleData> data = new();
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RoundEndedEvent>(OnRoundEnded);
    }

    private void OnRoundEnded(RoundEndedEvent ev)
    {
        foreach (var value in data.Values)
        {
            if (!value.IsForcedNow)
            {
                value.RoundMissed++;
            }

            if (value.RoundMissed == 4)
            {
                value.ForceCount = 0;
                value.RoundMissed = 0;
            }

            value.IsForcedNow = false;
        }
    }

    public bool AskForcePlay(IPlayerSession player, string role)
    {
        var playerData = data[player];
        if (playerData.ForceCount == 2)
            return false;

        playerData.ForceCount++;
        playerData.Role = role;
        playerData.IsForcedNow = true;

        return true;
    }

    public IPlayerSession? GetForcePlayForRole(string role)
    {
        foreach (var item in data)
        {
            if (item.Value.Role == role && item.Value.IsForcedNow)
                return item.Key;
        }

        return null;
    }

    private class PatronRoleData
    {
        public string Role;
        public int ForceCount;
        public int RoundMissed;
        public bool IsForcedNow = false;

        public PatronRoleData(string role, int forceCount, int roundMissed)
        {
            Role = role;
            ForceCount = forceCount;
            RoundMissed = roundMissed;
        }
    }
}
