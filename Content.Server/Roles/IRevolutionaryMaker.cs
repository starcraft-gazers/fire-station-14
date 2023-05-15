using Robust.Server.Player;

namespace Content.Server.Roles;

public interface IRevolutionaryMaker
{
    void Initialize();
    void MakeRevolutionary(IPlayerSession playerSession);
}
