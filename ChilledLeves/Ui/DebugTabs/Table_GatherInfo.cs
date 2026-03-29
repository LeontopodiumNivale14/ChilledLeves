using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_GatherInfo
    {
        private static readonly List<Job> gatherClasses = new() { Job.MIN, Job.BTN };

        public static void Draw()
        {
            if (ImGui.BeginTable("ChilledLeves: Debug Gathering Table", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("LeveId");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Map Info");

                foreach (var leve in LeveInfo.Leve_SheetInfo.Where(x => gatherClasses.Contains(x.Value.Job)))
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{leve.Key}");

                    ImGui.TableNextColumn();
                    var image = LeveInfo.Job_IconDict[leve.Value.Job].ColorIcon;
                    ImGui.Image(image.GetWrapOrEmpty().Handle, new Vector2(24, 24));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{leve.Value.LeveName}");

                    var mapInfo = leve.Value.Gather_MapInfo;

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"{leve.Value.LeveName}_Location"))
                    {
                        // Utils.SetFlagForNPC(mapInfo.TerritoryId, mapInfo.Location.X, mapInfo.Location.Z);
                        Utils.SetGatheringRingFromWorld(mapInfo.TerritoryId, mapInfo.Location, mapInfo.Radius, $"{leve.Value.LeveName}");
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"{mapInfo.Location.X}, {mapInfo.Location.Y}, {mapInfo.Location.Z}");
                        ImGui.EndTooltip();
                    }
                }

                ImGui.EndTable();
            }
        }
    }
}
