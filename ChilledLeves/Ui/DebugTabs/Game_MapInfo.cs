using ChilledLeves.Utilities;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using Lumina.Excel.Sheets.Experimental;
using System.Collections.Generic;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_MapInfo
    {
        private static List<MarkerData> _cachedMarkers = new();

        public static void Draw()
        {
            if (ImGui.Button("Refresh Markers"))
                _cachedMarkers = GetAllMarkers();

            ImGui.Separator();

            foreach (var marker in _cachedMarkers)
            {
                if (Svc.Texture.TryGetFromGameIcon(marker.IconId, out var icon))
                {
                    ImGui.Image(icon.GetWrapOrEmpty().Handle, new(24, 24));
                    ImGui.SameLine();
                }

                ImGui.Text($"{marker.Position} {marker.TooltipText}");
                ImGui.SameLine();
                if (ImGui.Button($"{marker.WorldPosition.X:N2}, {marker.WorldPosition.Y:N2}, {marker.WorldPosition.Z:N2}"))
                {
                    P.navmesh.PathfindAndMoveTo(marker.WorldPosition, true);
                }
            }

            // Debug: show raw values and player position for cross-referencing
            var player = Player.Object;
            ImGui.Text($"Player World Pos: {player.Position.X:N2}, {player.Position.Y:N2}, {player.Position.Z:N2}");

            var map = Player.Territory.Value.Map.Value;
            ImGui.Text($"Map SizeFactor: {map.SizeFactor}, OffsetX: {map.OffsetX}, OffsetY: {map.OffsetY}");

            ImGui.Separator();

            foreach (var marker in _cachedMarkers)
            {
                string text = $"Raw: {marker.Position.X}, {marker.Position.Y} | World: {marker.WorldPosition.X:N2}, {marker.WorldPosition.Y:N2}, {marker.WorldPosition.Z:N2} | {marker.TooltipText}";
                ImGui.Text(text);
                if (ImGui.IsItemClicked())
                {
                    ImGui.SetClipboardText(text);
                }
            }
        }

        private unsafe static List<MarkerData> GetAllMarkers()
        {
            var markers = new List<MarkerData>();
            var agentMap = AgentMap.Instance();
            if (agentMap == null) return markers;

            // Get the current map for coordinate conversion
            var map = Player.Territory.Value.Map;

            foreach (var marker in agentMap->MiniMapGatheringMarkers)
            {
                var markerInfo = marker.MapMarker;
                var text = marker.TooltipText.ToString();
                Vector2 markerPos = new(markerInfo.X, markerInfo.Y);

                if (markerPos == Vector2.Zero)
                    continue;

                Vector3 worldPos = Vector3.Zero;
                worldPos = MarkerToWorld3D(markerPos);

                markers.Add(new()
                {
                    IconId = markerInfo.IconId,
                    Position = new(markerInfo.X, markerInfo.Y),
                    WorldPosition = worldPos,
                    TooltipText = text
                });
            }

            return markers;
        }

        public class MarkerData
        {
            public Vector2 Position { get; set; }      // raw minimap coords
            public Vector3 WorldPosition { get; set; } // converted world space (Y = 0 until resolved)
            public uint IconId { get; set; }
            public string TooltipText { get; set; } = string.Empty;
        }

        public static Vector3 MarkerToWorld3D(Vector2 MapMarker, float worldY = 0f)
        {
            float x = MapMarker.X / 16f;
            float z = MapMarker.Y / 16f;

            var floor = P.navmesh.NearestPointReachable(new(x, 0, z), 1024, 1024);
            return floor ?? new Vector3(x, worldY, z);
        }
    }
}
