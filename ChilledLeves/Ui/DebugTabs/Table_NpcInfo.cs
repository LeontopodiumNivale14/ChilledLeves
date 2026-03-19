using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.OldUtils;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_NpcInfo
    {
        private static string _nameSearch = "";


        public static void Draw()
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Name Search", ref _nameSearch);

            if (ImGui.BeginTable("ChilledLeves: Npc Info", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Location");

                foreach (var entry in LeveInfo.LeveNpc_Info.OrderBy(x => x.Value.TerritoryId))
                {
                    if (!Old_Utils.ContainsIgnoreSpacesAndCase(entry.Value.Name, _nameSearch))
                        continue;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{entry.Value.Name}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{TerritoryName.GetTerritoryName(entry.Value.TerritoryId)}");

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag: {entry.Key}"))
                    {
                        SetFlagForNPC(entry.Value.TerritoryId, entry.Value.Npc_Flag.X, entry.Value.Npc_Flag.Y);
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
