using Content.Shared.Preferences;
using Robust.Server.Player;

namespace Content.Server.Roles;

public interface IAntagManager
{
    public List<IPlayerSession> GetPreferedAntags(List<IPlayerSession> players, int requiredCount);

    public Dictionary<IPlayerSession, HumanoidCharacterProfile> GetPreferedAntags(Dictionary<IPlayerSession, HumanoidCharacterProfile> players, int requiredCount);
}
