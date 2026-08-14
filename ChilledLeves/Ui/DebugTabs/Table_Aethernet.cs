using ChilledLeves.Scheduler.Tasks;
using ChilledLeves.Utilities;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_Aethernet
    {
        public static void Draw()
        {
            var currentTerritory = Player.Territory.RowId;

            var aetherShards = Utils.Aethernet.Where(x => x.Value.ValidTerritories.Contains(currentTerritory)).ToList();
            if (aetherShards.Count != 0)
            {
                using (var table = ImRaii.Table("Aethernet Info", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Position");
                    ImGui.TableSetupColumn("Distance to");
                    ImGui.TableHeadersRow();

                    foreach (var entry in aetherShards)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{entry.Key}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Position}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"Calculate##DistanceTo_{entry.Key}"))
                        {
                            var playerPos = Player.Position;

                            entry.Value.DistanceTo = Task.Run(async () => await Task_Navmesh.FindPath(playerPos, entry.Value.MoveTo))
                        }
                    }

                }
            }
            else
            {
                ImGui.Text("There are no valid aethershards here.");
            }
        }
    }
}
