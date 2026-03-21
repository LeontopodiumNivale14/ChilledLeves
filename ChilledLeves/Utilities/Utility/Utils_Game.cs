using FFXIVClientStructs.FFXIV.Client.UI.Agent;
namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public static unsafe void SetGatheringRing(uint territoryId, int x, int y, int radius, string? tooltip = "Node Location")
    {
        var map = ExcelHelper.Sheet_TerritoryType.GetRow(territoryId).Map.Value;
        var agent = AgentMap.Instance();

        Vector2 pos = MapToWorld(new Vector2(x, y), map.SizeFactor, map.OffsetX, map.OffsetY);
        // IceLogging.Debug($"Current map: {map.RowId} {territoryId} | {map.PlaceName.Value.Name} | {pos.X} {pos.Y} | {x} {y} | {radius} | {tooltip}");

        agent->FlagMarkerCount = 0;
        agent->SetFlagMapMarker(territoryId, map.RowId, x, y);
        agent->TempMapMarkerCount = 0;
        agent->AddGatheringTempMarker(x, y, radius, tooltip: tooltip);
        agent->OpenMap(map.RowId, territoryId, tooltip, MapType.GatheringLog);
    }
    public static unsafe void SetGatheringRingFromWorld(uint territoryId, Vector3 worldPos, int radius, string? tooltip = "Node Location")
    {
        var map = ExcelHelper.Sheet_TerritoryType.GetRow(territoryId).Map.Value;
        var agent = AgentMap.Instance();

        // Use the built-in Vector3 overload which handles world→map conversion correctly
        agent->FlagMarkerCount = 0;
        agent->SetFlagMapMarker(territoryId, map.RowId, worldPos);

        // Read back the converted coords from the flag marker itself
        var flag = agent->FlagMapMarkers[0];
        int mapX = (int)MathF.Round(flag.MapMarker.X / 16f);
        int mapY = (int)MathF.Round(flag.MapMarker.Y / 16f);

        agent->TempMapMarkerCount = 0;
        agent->AddGatheringTempMarker(mapX, mapY, radius * 10, tooltip: tooltip);
        agent->OpenMap(map.RowId, territoryId, tooltip, MapType.GatheringLog);
    }
    public static Vector2 MapToWorld(Vector2 coordinates, ushort sizeFactor, short offsetX, short offsetY)
    {
        float WorldConverter(float value, uint scale, int offset) => -offset * (scale / 100.0f) + 50.0f * (value - 1) * (scale / 100.0f);

        var scalar = sizeFactor / 100.0f;

        var xWorldCoord = WorldConverter(coordinates.X, sizeFactor, offsetX);
        var yWorldCoord = WorldConverter(coordinates.Y, sizeFactor, offsetY);

        var objectPosition = new Vector2(xWorldCoord, yWorldCoord);
        var center = new Vector2(1024.0f, 1024.0f);

        return objectPosition / scalar - center / scalar;
    }
    public static Vector2 WorldToMap(Vector2 worldCoords, ushort sizeFactor, short offsetX, short offsetY)
    {
        float scalar = sizeFactor / 100.0f;
        float center = 1024.0f / scalar;

        float MapConverter(float world, int offset) =>
            (world + center + offset) / 50.0f + 1.0f;

        return new Vector2(
            MapConverter(worldCoords.X, offsetX),
            MapConverter(worldCoords.Y, offsetY)
        );
    }
    public static unsafe void SetFlagForNPC(uint territoryId, float x, float y)
    {
        var mapId = ExcelHelper.Sheet_TerritoryType.GetRow(territoryId).Map.Value.RowId;

        var agent = AgentMap.Instance();

        agent->FlagMarkerCount = 0;
        agent->SetFlagMapMarker(territoryId, mapId, x, y);
        agent->OpenMapByMapId(mapId, territoryId: territoryId);
    }
    public static unsafe uint CurrentMap()
    {
        var agent = AgentMap.Instance();
        return agent->CurrentMapId;
    }

}
