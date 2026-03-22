using ChilledLeves.Enums;
using ChilledLeves.Resources;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.ImGuiFileDialog;
using ECommons.GameHelpers;
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
        private static bool AddNodesPassively = true;
        private static uint SelectedNodeId = 0;

        private static string VfxPath = "general01bf";

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

            if (ImGui.BeginTable("ChilledLeves: Route Editors", 2, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("Selector Window");
                ImGui.TableSetupColumn("Detailed View", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                var height = ImGui.GetContentRegionAvail().Y - 20;

                if (ImGui.BeginChild("Debug: Route Selector", new(200, height), true))
                {
                    var gatherRoutes = RouteLoader.Leve_Routes.OrderBy(x => x.Value.ExpansionId)
                                          .ThenBy(x => x.Value.TerritoryId)
                                          .ThenBy(x => x.Value.GatheringJob);

                    uint previousTer = 0;
                    var expansion = ExpansionIds.Unk;

                    foreach (var route in gatherRoutes)
                    {
                        if (expansion != route.Value.ExpansionId)
                        {
                            ImGui.Text($"- - {route.Value.ExpansionId} - - ");
                            expansion = route.Value.ExpansionId;
                        }

                        var territoryId = route.Value.TerritoryId;
                        if (territoryId != previousTer)
                        {
                            string territoryName = ExcelHelper.GetTerritoryName(territoryId);

                            ImGui.Text($"{territoryName}");
                            previousTer = territoryId;
                        }
                        bool isSelected = LeveSelected == route.Key;
                        var label = isSelected ? $"→ [{route.Key}] {route.Value.GatheringJob}" : $"[{route.Key}] {route.Value.GatheringJob}";
                        ;
                        if (ImGui.Selectable(label, isSelected))
                        {
                            LeveSelected = route.Key;
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.TableNextColumn();
                var remainingSpace = ImGui.GetContentRegionAvail().X;
                if (ImGui.BeginChild("Debug: Route Editor itself", new(remainingSpace, height), true))
                {
                    if (LeveInfo.Leve_SheetInfo.TryGetValue(LeveSelected, out var sheetInfo))
                    {
                        var routeInfo = RouteLoader.GetRoute(LeveSelected);

                        var mapInfo = sheetInfo.Gather_MapInfo;
                        var leveName = sheetInfo.LeveName;

                        #region Name + Buttons
                        Theme_Colors.BodyText($"[{LeveSelected}] → {leveName}");
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"{leveName}_Location"))
                        {
                            Utils.SetGatheringRingFromWorld(mapInfo.TerritoryId, mapInfo.Location, mapInfo.Radius, $"{leveName}");
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Leve Flag");
                        }

                        ImGui.SameLine();

                        if (ImGuiEx.IconButton(FontAwesomeIcon.TrainTram, $"{leveName}_Teleport"))
                        {
                            P.navmesh.PathToFlag();
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("SmartNav PathTo");
                        }

                        ImGui.SameLine();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Square, $"{leveName}_Stop"))
                        {
                            if (P.navmesh.SmartIsRunning())
                                P.navmesh.SmartNavStop();
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Stop SmartNav");
                        }

                        ImGui.SameLine();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Save, $"{leveName}_Save"))
                        {
                            RouteLoader.SaveRoute(routeInfo, C.RouteSaveLocation);
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Save Route");
                        }

                        ImGui.SameLine();
                        ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
                        if (ImGui.IsItemHovered())
                        {
                            var nodeIds = sheetInfo.Gather_NodeInfo.NodeIds.OrderBy(x => x);
                            string gatherPointIds = string.Join(", ", nodeIds);
                            ImGui.SetTooltip($"[{nodeIds.Count()}] {gatherPointIds}");
                        }
                        ImGui.Text("");

                        #endregion

                        if (ImGui.CollapsingHeader("Picto"))
                        {
                            var selectedColor = C.Picto_GatherBase;
                            if (ImGui.ColorEdit4("Selected Color", ref selectedColor, ImGuiColorEditFlags.PickerHueWheel))
                            {
                                C.Picto_GatherBase = selectedColor;
                                C.SaveDebounced();
                            }

                            ImGui.InputText("Vfx Path", ref VfxPath);
                        }

                        if (ImGui.BeginTable("Route Details: Editor Stuff", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                        {
                            ImGui.TableSetupColumn("NodeIds");
                            ImGui.TableSetupColumn("Node Editor", ImGuiTableColumnFlags.WidthStretch);

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            #region Node Selection

                            ImGui.Checkbox("Passively Add Nodes", ref AddNodesPassively);

                            if (AddNodesPassively)
                            {
                                foreach (var node in Svc.Objects
                                    .Where(x => sheetInfo.Gather_NodeInfo.NodeIds.Contains(x.BaseId))
                                    .Where(x => x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.GatheringPoint))
                                {
                                    if (!routeInfo.NodeInfo.Any(x => x.BaseId == node.BaseId))
                                    {
                                        routeInfo.NodeInfo.Add(new()
                                        {
                                            BaseId = node.BaseId,
                                            Position = node.Position,
                                            Gathering_FanInfo = new(),
                                            Flight_FanInfo = new(),
                                        });
                                    }
                                }
                            }

                            ImGui.Text($"Count: {routeInfo.NodeInfo.Count()}");
                            if (routeInfo.NodeInfo.Count() == sheetInfo.Gather_NodeInfo.NodeIds.Count())
                            {
                                ImGui.SameLine();
                                ImGuiEx.Icon(FontAwesomeIcon.Check);
                                ImGui.Text($"");
                            }
                            foreach (var node in routeInfo.NodeInfo)
                            {
                                bool isSelected = node.BaseId == SelectedNodeId;
                                var label = isSelected ? $"→ [{node.BaseId}]" : $"[{node.BaseId}]";
                                if (ImGui.Selectable(label, isSelected))
                                {
                                    SelectedNodeId = node.BaseId;
                                }

                                PictoManager.DrawGatheringFan(node, isSelected);
                                PictoManager.TestVfxCircle($"{node.BaseId}", node.Position, C.Picto_GatherBase, VfxPath);
                            }

                            #endregion

                            ImGui.TableNextColumn();
                            if (routeInfo.NodeInfo.TryGetFirst(x => x.BaseId == SelectedNodeId, out var nodeInfo))
                            {
                                ImGui.Text($"Node: {SelectedNodeId}");
                                if (ImGui.Button($"{nodeInfo.Position.X:N2}, {nodeInfo.Position.Y:N2}, {nodeInfo.Position.Z:N2}"))
                                {
                                    P.navmesh.SmartPathTo(routeInfo.TerritoryId, nodeInfo.Position);
                                }
                            }

                            ImGui.EndTable();
                        }

                    }
                    else
                    {
                        ImGui.Text($"Leve not in here? {LeveSelected}");
                    }
                }
                ImGui.EndChild();

                ImGui.EndTable();
            }

            fileDialogManager.Draw();
        }
    }
}
