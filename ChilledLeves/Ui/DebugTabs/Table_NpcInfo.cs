using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_NpcInfo
    {
        private static string _nameSearch = "";
        private static uint selectedNpc = 0;

        public static void Draw()
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Name Search", ref _nameSearch);

            if (ImGui.BeginTable("ChilledLeves: Npc Info", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Location");

                foreach (var entry in LeveInfo.LeveNpc_Info.OrderBy(x => x.Value.TerritoryId))
                {
                    if (!Utils.ContainsIgnoreSpacesAndCase(entry.Value.Name, _nameSearch))
                        continue;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    if (selectedNpc == entry.Key)
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                    }


                    ImGui.Text($"{entry.Value.Name}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{TerritoryName.GetTerritoryName(entry.Value.TerritoryId)}");

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag: {entry.Key}"))
                    {
                        Utils.SetFlagForNPC(entry.Value.TerritoryId, entry.Value.Npc_Flag.X, entry.Value.Npc_Flag.Y);
                    }

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plane, $"TP {entry.Key}"))
                    {
                        selectedNpc = entry.Key;
                        P.navmesh.Smart_PathToPoint(entry.Value.TerritoryId, entry.Value.Npc_InteractZone);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Territory: {entry.Value.TerritoryId}");
                        ImGui.Text($"Position: {entry.Value.Npc_InteractZone}");
                        ImGui.EndTooltip();
                    }
                    ImGui.SameLine();

                    if (ImGuiEx.IconButton(FontAwesomeIcon.Running, $"Travel {entry.Key}"))
                    {
                        selectedNpc = entry.Key;
                        P.navmesh.PathfindAndMoveTo(entry.Value.Npc_InteractZone, false);
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
