using System;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Roles;
using Content.Shared.CCVar;
using Content.Shared.Preferences;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Content.Server.Roles;

//Фильтрация антагов для выбора
//В базовой реализации, если после фильтрации не хватает игроков, просто возвращаем текущий список
public sealed class AntagManager : IAntagManager
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    private bool IsAntagsBlockedByTime = true;
    private bool IsGhostRolesBlockedByTime = true;
    private int MinTimeToPlayAntag = 1800;
    private int MinTimeToPlayGhostRole = 1800;

    public void Initialize()
    {
        IsAntagsBlockedByTime = _config.GetCVar(CCVars.IsAntagsBlockedByTime);
        IsGhostRolesBlockedByTime = _config.GetCVar(CCVars.IsGhostRolesBlockedByTime);
        MinTimeToPlayAntag = _config.GetCVar(CCVars.MinTimeToPlayAntag);
        MinTimeToPlayGhostRole = _config.GetCVar(CCVars.MinTimeToPlayGhostRole);

        _config.OnValueChanged(CCVars.IsAntagsBlockedByTime, IsAntagsBlockedByTimeChanged, true);
        _config.OnValueChanged(CCVars.MinTimeToPlayAntag, MinTimeToPlayAntagChanged, true);
        _config.OnValueChanged(CCVars.IsGhostRolesBlockedByTime, GhostAntagBlockedChanged, true);
        _config.OnValueChanged(CCVars.MinTimeToPlayGhostRole, MinTimeToPlayGhostRoleChanged, true);
    }

    private void MinTimeToPlayGhostRoleChanged(int obj)
    {
        MinTimeToPlayGhostRole = obj;
    }

    private void GhostAntagBlockedChanged(bool obj)
    {
        IsGhostRolesBlockedByTime = obj;
    }

    private void MinTimeToPlayAntagChanged(int obj)
    {
        MinTimeToPlayAntag = obj;
    }

    private void IsAntagsBlockedByTimeChanged(bool obj)
    {
        IsAntagsBlockedByTime = obj;
    }

    public List<IPlayerSession> GetPreferedAntags(List<IPlayerSession> players, int requiredCount)
    {
        if (!IsAntagsBlockedByTime)
            return players;


        var filteredPlayers = players
            .Where(player => IsValidPlayedTime(player, MinTimeToPlayAntag))
            .ToList();

        if (filteredPlayers.Count() < requiredCount)
            return players;

        return filteredPlayers;
    }

    public Dictionary<IPlayerSession, HumanoidCharacterProfile> GetPreferedAntags(Dictionary<IPlayerSession, HumanoidCharacterProfile> players, int requiredCount)
    {
        if (!IsAntagsBlockedByTime)
            return players;

        var filtered = players
            .Where(keyValue => IsValidPlayedTime(keyValue.Key, MinTimeToPlayAntag))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        if (filtered.Count() < requiredCount)
            return players;

        return filtered;
    }

    public bool IsPlayerTimeValidForGhostRole(IPlayerSession player)
    {
        if (!IsGhostRolesBlockedByTime)
            return true;

        return IsValidPlayedTime(player, MinTimeToPlayGhostRole);
    }

    private bool IsValidPlayedTime(IPlayerSession player, int requiredTime)
    {
        try
        {
            //Чтобы оставить стандартное поведение
            if (!player.ConnectedClient.IsConnected || player.Status == Robust.Shared.Enums.SessionStatus.Disconnected)
                return true;

            var values = _playTimeTracking.GetTrackerTimes(player);
            var playedTime = 0d;

            foreach (var item in values)
            {
                playedTime += item.Value.TotalMinutes;
            }

            _logManager.GetSawmill("AntagManager").Info($"Player Time for {player.Name} is - {playedTime}");
            return playedTime > requiredTime;
        }
        catch (Exception ex)
        {
            return true;
        }
    }
}
