using System.Text.Json.Nodes;

namespace Content.Server.GameTicking;

public interface IStatusResponseProvider
{
    void GetStatusResponse(JsonNode jObject, GameRunLevel runLevel, DateTime roundStartDateTime);
}
