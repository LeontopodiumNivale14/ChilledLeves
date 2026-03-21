using ChilledLeves.Enums;
using ChilledLeves.Resources;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.ImGuiFileDialog;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Data.Files.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_GatherEditor
    {
        private static FileDialogManager fileDialogManager = new FileDialogManager();
        private static readonly List<LeveClass> gatherClasses = new() { LeveClass.Min, LeveClass.Btn };

        private static uint LeveSelected = 0;

        public static void Draw()
        {
            var AllRoutes = RouteLoader.Leve_Routes;

            if (ImGui.Button("Select Export Folder"))
            {
                fileDialogManager.OpenFolderDialog("Select Export Folder", (success, path) =>
                {
                    if (success && !string.IsNullOrEmpty(path))
                    {
                        C.RouteSaveLocation = path;
                        C.Save();
                        PluginLog.Information($"Export path set to: {path}");
                    }
                });
            }
            ImGui.SameLine();
            ImGui.Text($"{C.RouteSaveLocation}");

            if (ImGui.Button("Create All Routes"))
            {
                foreach (var gatherRoute in LeveInfo.Leve_SheetInfo.Where(x => gatherClasses.Contains(x.Value.Job)))
                {
                    if (!AllRoutes.ContainsKey(gatherRoute.Key))
                    {
                        var territoryId = LeveInfo.Leve_SheetInfo[gatherRoute.Key].Gather_MapInfo.TerritoryId;
                        var routeName = ExcelHelper.Sheet_TerritoryType.GetRow(territoryId).PlaceNameZone.Value.Name.ToString();

                        AllRoutes.Add(gatherRoute.Key, new()
                        {
                            LeveId = gatherRoute.Key,
                            TerritoryId = territoryId,
                            ZoneName = routeName,
                            GatheringJob = gatherRoute.Value.Job,
                            NodeInfo = new(),
                            ExpansionId = gatherRoute.Value.Expansion,
                        });
                    }
                }

                RouteLoader.SaveAllRoutes(C.RouteSaveLocation);
            }

            if (ImGui.BeginTable("ChilledLeves: Route Editors", 2))
            {
                ImGui.TableSetupColumn("");

                ImGui.EndTable();
            }

            fileDialogManager.Draw();
        }
    }
}
