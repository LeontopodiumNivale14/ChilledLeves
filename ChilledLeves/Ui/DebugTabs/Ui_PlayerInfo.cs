using ChilledLeves.Utilities.LeveData;
using ECommons.GameHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_PlayerInfo
    {
        public class TargetInfo
        {
            public uint BaseId { get; set; } = 0;
            public string Name { get; set; } = "???";
            public Vector3 Position { get; set; } = new();
        }

        public static TargetInfo? LastTarget = null;

        public static void Draw()
        {
            var TargetInfo = new TargetInfo();
            if (Player.Available)
            {
                if (Svc.Objects.LocalPlayer.TargetObject != null)
                {
                    if (ImGui.Button("Copy Aethershard"))
                    {
                        ImGui.SetClipboardText(CopyAethernet());
                    }

                    var lastTarget = Svc.Objects.LocalPlayer.TargetObject;
                    LastTarget = new()
                    {
                        BaseId = lastTarget.BaseId,
                        Name = lastTarget.Name.ToString(),
                        Position = lastTarget.Position,
                    };
                }
            }

            if (ImGui.BeginTable("Player Detailed View", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Details");
                ImGui.TableSetupColumn("Info", ImGuiTableColumnFlags.WidthStretch);

                if (Player.Available)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Player Info");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Position");

                    ImGui.TableNextColumn();
                    var PlayerPos = Player.Position;
                    if (ImGui.Button($"X:{PlayerPos.X:N2}, Y:{PlayerPos.Y:N2}, Z:{PlayerPos.Z:N2}"))
                    {
                        ImGui.SetClipboardText($"{PlayerPos.X:N2}f, {PlayerPos.Y:N2}f, {PlayerPos.Z:N2}f");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($"Territory");

                    ImGui.TableNextColumn();
                    ImGui.Text($"[{Player.Territory.RowId}] {Player.Territory.Value.PlaceName.Value.Name}");
                }

                if (LastTarget != null)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Last Target Info");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Name");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{LastTarget.Name}");

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($"ID:");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{LastTarget.BaseId}");

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text($"Position");

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"X:{LastTarget.Position.X:N2}, Y:{LastTarget.Position.Y:N2}, Z:{LastTarget.Position.Z:N2}"))
                    {
                        ImGui.SetClipboardText($"{LastTarget.Position.X:N2}f, {LastTarget.Position.Y:N2}f, {LastTarget.Position.Z:N2}f");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"Distance To Player");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{Player.DistanceTo(LastTarget.Position):N2}");
                }

                ImGui.EndTable();
            }
        }

        public static string CopyAethernet()
        {
            var sb = new StringBuilder();
            if (Player.Available)
            {
                if (Svc.Objects.LocalPlayer.TargetObject != null)
                {
                    var lastTarget = Svc.Objects.LocalPlayer.TargetObject;
                    var territory = Player.Territory.RowId;
                    var position = Player.Position;

                    sb.AppendLine($"[{lastTarget.BaseId}] = new()");
                    sb.AppendLine("{");
                    sb.AppendLine($"\tShardId = {lastTarget.BaseId},");
                    sb.AppendLine($"\tTerritoryId = {territory},");
                    sb.AppendLine($"\tValidTerritories = new() {{{territory}}},");
                    sb.AppendLine($"\tPosition = new({lastTarget.Position.X:N2}f, {lastTarget.Position.Y:N2}f, {lastTarget.Position.Z:N2}f),");
                    sb.AppendLine($"\tMoveTo = new({position.X:N2}f, {position.Y:N2}f, {position.Z:N2}f),");
                    sb.AppendLine("},");
                }
            }

            return sb.ToString();
        }
    }
}
