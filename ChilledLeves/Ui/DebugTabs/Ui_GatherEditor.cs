using ChilledLeves.Enums;
using ChilledLeves.Resources;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_GatherEditor
    {
        private static FileDialogManager fileDialogManager = new FileDialogManager();
        private static readonly List<Job> gatherClasses = new() { Job.MIN, Job.BTN };

        private static uint LeveSelected = 0;
        private static bool AddNodesPassively = true;
        private static uint SelectedNodeId = 0;

        private static List<string> OmenNames = new();
        private static int OmenPage = 0;
        private static int OmenIndex = 0;

        private static string VfxPath = "k5d1_omen_o01pg";

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

                        PictoEditor();

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
                                PictoManager.TestVfxCircle($"{node.BaseId}", node.Position, isSelected, VfxPath);
                            }

                            #endregion

                            ImGui.TableNextColumn();
                            NodeEditor(routeInfo);

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

        public static void PictoEditor()
        {
            if (ImGui.CollapsingHeader("Picto"))
            {
                var selectedColor = C.Picto_SelectedGatherBase;
                if (ImGui.ColorEdit4("Selected Color", ref selectedColor, ImGuiColorEditFlags.PickerHueWheel))
                {
                    C.Picto_SelectedGatherBase = selectedColor;
                    C.SaveDebounced();
                }

                var gatherBase = C.Picto_GatherBase;
                if (ImGui.ColorEdit4("Non-Selected Color", ref gatherBase, ImGuiColorEditFlags.PickerHueWheel))
                {
                    C.Picto_GatherBase = gatherBase;
                    C.SaveDebounced();
                }



                // Ensure populated
                if (OmenNames.Count == 0)
                    foreach (var omen in ExcelHelper.Sheet_Omen)
                        OmenNames.Add(omen.Path.ToString());

                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.ArrowLeft, "##PreviousOmen_Debug"))
                {
                    OmenIndex = OmenIndex <= 0 ? OmenNames.Count - 1 : OmenIndex - 1;
                    VfxPath = OmenNames[OmenIndex];
                    OmenPage = OmenIndex / 20;
                }

                ImGui.SameLine();
                ImGui.Text($"[{OmenIndex}]");
                ImGui.SameLine();
                if (ImGui.Button("Select Vfx"))
                    ImGui.OpenPopup("Debug: Vfx Selector");

                ImGui.SameLine();
                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.ArrowRight, "##NextOmen_Debug"))
                {
                    OmenIndex = OmenIndex >= OmenNames.Count - 1 ? 0 : OmenIndex + 1;
                    VfxPath = OmenNames[OmenIndex];
                    OmenPage = OmenIndex / 20;
                }

                if (ImGui.BeginPopup("Debug: Vfx Selector"))
                {
                    if (OmenNames.Count == 0)
                    {
                        foreach (var omen in ExcelHelper.Sheet_Omen)
                            OmenNames.Add(omen.Path.ToString());
                    }

                    const int pageSize = 20;
                    var totalPages = (OmenNames.Count + pageSize - 1) / pageSize; // ceiling division

                    // Page controls
                    ImGui.BeginDisabled(OmenPage <= 0);
                    if (ImGui.Button("##prev")) OmenPage--;
                    ImGui.EndDisabled();

                    ImGui.SameLine();
                    ImGui.Text($"Page {OmenPage + 1} / {totalPages}");
                    ImGui.SameLine();

                    ImGui.BeginDisabled(OmenPage >= totalPages - 1);
                    if (ImGui.Button("##next")) OmenPage++;
                    ImGui.EndDisabled();

                    // Optional: jump to page input
                    ImGui.SameLine();
                    var pageInput = OmenPage + 1;
                    ImGui.SetNextItemWidth(50);
                    if (ImGui.InputInt("##page", ref pageInput, 0))
                        OmenPage = Math.Clamp(pageInput - 1, 0, totalPages - 1);

                    if (ImGui.BeginTable("Debug: Omen Selector", 2,
                        ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                    {
                        ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.WidthFixed, 50);
                        ImGui.TableSetupColumn("Path", ImGuiTableColumnFlags.WidthStretch);
                        ImGui.TableHeadersRow();

                        var start = OmenPage * pageSize;
                        var end = Math.Min(start + pageSize, OmenNames.Count);

                        for (var i = start; i < end; i++)
                        {
                            ImGui.TableNextRow();

                            // Column 1: index
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text($"{i}");

                            // Column 2: selectable spanning full row
                            ImGui.TableSetColumnIndex(1);
                            var isSelected = VfxPath == OmenNames[i];
                            if (ImGui.Selectable(OmenNames[i] + $"##{i}", isSelected, ImGuiSelectableFlags.SpanAllColumns))
                            {
                                OmenIndex = i;
                                VfxPath = OmenNames[i];
                                ImGui.CloseCurrentPopup();
                            }
                        }

                        ImGui.EndTable();
                    }

                    ImGui.EndPopup();
                }

                ImGui.Text($"{VfxPath}");
            }
        }

        public static void NodeEditor(GatheringRoute routeInfo)
        {
            if (routeInfo.NodeInfo.TryGetFirst(x => x.BaseId == SelectedNodeId, out var nodeInfo))
            {
                ImGui.Text($"Node: {SelectedNodeId}");
                if (ImGui.Button($"{nodeInfo.Position.X:N2}, {nodeInfo.Position.Y:N2}, {nodeInfo.Position.Z:N2}"))
                {
                    P.navmesh.Smart_PathToPoint(routeInfo.TerritoryId, nodeInfo.Position);
                }

                ImGui.Separator();

                #region Gathering Fan Info

                ImGui.Separator();
                ImGui.Text($"Gathering Location");
                var gatherInfo = nodeInfo.Gathering_FanInfo;

                var gather_FanStart = gatherInfo.Fan_StartAngle;
                var gather_FanEnd = gatherInfo.Fan_EndAngle;
                var gather_FanMin = gatherInfo.Fan_DistanceMin;
                var gather_FanMax = gatherInfo.Fan_DistanceMax;
                var gather_Height = gatherInfo.Fan_Height;

                ImGui.PushID("Fan: Gathering");

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Start", ref gather_FanStart, 1, 0, 360))
                {
                    gatherInfo.Fan_StartAngle = gather_FanStart;
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("End", ref gather_FanEnd, 1, 0, 360))
                {
                    gatherInfo.Fan_EndAngle = gather_FanEnd;
                }

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Min Distance", ref gather_FanMin, 0.2f, 0, 4))
                {
                    gatherInfo.Fan_DistanceMin = gather_FanMin;
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Max Distance", ref gather_FanMax, 0.2f, 0, 4))
                {
                    gatherInfo.Fan_DistanceMax = gather_FanMax;
                }

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Height", ref gather_Height, 0.1f, 0, 5))
                {
                    gatherInfo.Fan_Height = gather_Height;
                }

                using (var disabled = ImRaii.Disabled(_isGeneratingFan || !ImGui.IsKeyDown(ImGuiKey.LeftShift)))
                {
                    if (ImGui.Button("Generate Fan from Navmesh"))
                    {
                        _ = GenerateFanForNode(nodeInfo);
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Pathfind to fan"))
                {
                    var randomPos = Utils.GetRandomGatherPosition(nodeInfo, Player.Position);
                    P.navmesh.PathfindAndMoveTo(randomPos, true);
                }

                ImGui.PopID();

                #endregion

                #region Flight Fan Info

                ImGui.Separator();
                var flightInfo = nodeInfo.Flight_FanInfo;

                var flight_FanStart = flightInfo.Fan_StartAngle;
                var flight_FanEnd = flightInfo.Fan_EndAngle;
                var flight_FanMin = flightInfo.Fan_DistanceMin;
                var flight_FanMax = flightInfo.Fan_DistanceMax;
                var flight_Height = flightInfo.Fan_Height;

                ImGui.PushID($"Fan: Flight");

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Start", ref flight_FanStart, 1, 0, 360))
                {
                    flightInfo.Fan_StartAngle = flight_FanStart;
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("End", ref flight_FanEnd, 1, 0, 360))
                {
                    flightInfo.Fan_EndAngle = flight_FanEnd;
                }

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Min Distance", ref flight_FanMin, 0.2f, 0, 4))
                {
                    flightInfo.Fan_DistanceMin = flight_FanMin;
                }

                ImGui.SameLine();
                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Max Distance", ref flight_FanMax, 0.2f, 0, 4))
                {
                    flightInfo.Fan_DistanceMax = flight_FanMax;
                }

                ImGui.SetNextItemWidth(100);
                if (ImGui.DragFloat("Height", ref flight_Height, 0.1f, 0, 5))
                {
                    flightInfo.Fan_Height = flight_Height;
                }

                using (var disabled = ImRaii.Disabled(_isGeneratingFan || !ImGui.IsKeyDown(ImGuiKey.LeftShift)))
                {
                    if (ImGui.Button("Generate Fan from Navmesh"))
                    {
                        _ = GenerateFlightFanForNode(nodeInfo);
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Match to Gathering Fan"))
                    {
                        flightInfo.Fan_DistanceMin = gather_FanMax + 0.2f;
                        flightInfo.Fan_DistanceMax = 4f;
                        flightInfo.Fan_StartAngle = gather_FanStart;
                        flightInfo.Fan_EndAngle = gather_FanEnd;
                        flightInfo.Fan_Height = gather_Height;
                    }
                }

                ImGui.SameLine();
                if (ImGui.Button("Pathfind to fan"))
                {
                    var randomPos = Utils.GetRandomFlightPosition(nodeInfo, Player.Position);
                    P.navmesh.PathfindAndMoveTo(randomPos, true);
                }

                ImGui.PopID();


                #endregion
            }
        }

        #region Fan Generation Code

        private static bool _isGeneratingFan = false;
        private static string _fanGenStatus = string.Empty;
        private static async Task GenerateFanForNode(GatheringNode route)
        {
            _isGeneratingFan = true;
            _fanGenStatus = string.Empty;

            string tag = "Task: Gathering Fan Generation";

            try
            {
                Vector3 nodePos = route.Position;

                const float snapToleranceXZ = 0.5f;
                const float snapToleranceY = 5f;
                const float testDistanceMin = 1.0f;
                const float testDistanceMax = 2.4f;
                const float distanceStep = 0.5f;
                const int angleSamples = 360;

                var validDistances = new Dictionary<int, List<float>>();
                var validYHeights = new Dictionary<int, float>();

                await Task.Run(() =>
                {
                    for (int angleDeg = 0; angleDeg < angleSamples; angleDeg++)
                    {
                        bool allDistancesValid = true;
                        var distancesForAngle = new List<float>();
                        float highestY = float.MinValue;

                        for (float dist = testDistanceMin; dist <= testDistanceMax; dist += distanceStep)
                        {
                            float standardAngle = 180f - angleDeg;
                            float rad = standardAngle * (MathF.PI / 180f);
                            Vector3 candidate = new Vector3(
                                nodePos.X + dist * MathF.Sin(rad),
                                nodePos.Y,
                                nodePos.Z + dist * MathF.Cos(rad)
                            );

                            var nearest = P.navmesh.NearestPointReachable(candidate, snapToleranceXZ, snapToleranceY);
                            if (nearest.HasValue)
                            {
                                float xzDist = MathF.Sqrt(
                                    MathF.Pow(nearest.Value.X - candidate.X, 2) +
                                    MathF.Pow(nearest.Value.Z - candidate.Z, 2)
                                );
                                float yDist = MathF.Abs(nearest.Value.Y - candidate.Y);

                                if (xzDist <= snapToleranceXZ && yDist <= snapToleranceY)
                                {
                                    distancesForAngle.Add(dist);
                                    if (nearest.Value.Y > highestY)
                                        highestY = nearest.Value.Y;
                                }
                                else
                                {
                                    allDistancesValid = false;
                                    break;
                                }
                            }
                            else
                            {
                                allDistancesValid = false;
                                break;
                            }
                        }

                        if (allDistancesValid && distancesForAngle.Count > 0)
                        {
                            validDistances[angleDeg] = distancesForAngle;
                            validYHeights[angleDeg] = highestY;
                        }
                    }
                });

                if (validDistances.Count == 0)
                {
                    _fanGenStatus = "No reachable points found around this node.";
                    return;
                }

                bool[] valid = new bool[360];
                foreach (var kvp in validDistances)
                    valid[kvp.Key] = true;

                int bestStart = 0, bestLen = 0;
                int currentStart = 0, currentLen = 0;

                for (int i = 0; i < 720; i++)
                {
                    if (valid[i % 360])
                    {
                        if (currentLen == 0)
                            currentStart = i;
                        currentLen++;

                        if (currentLen > bestLen)
                        {
                            bestLen = currentLen;
                            bestStart = currentStart;
                        }
                    }
                    else
                    {
                        currentLen = 0;
                    }

                    if (currentLen >= 360)
                        break;
                }

                if (bestLen == 0)
                {
                    _fanGenStatus = "Could not find a contiguous arc of reachable angles.";
                    return;
                }

                int ffxivStart = bestStart % 360;
                int ffxivEnd = (bestStart + bestLen - 1) % 360;

                float allMin = float.MaxValue, allMax = float.MinValue;
                float arcMaxY = float.MinValue;

                foreach (var kvp in validDistances)
                {
                    int normalizedAngle = ((kvp.Key - ffxivStart) % 360 + 360) % 360;
                    if (normalizedAngle < bestLen)
                    {
                        foreach (var d in kvp.Value)
                        {
                            if (d < allMin) allMin = d;
                            if (d > allMax) allMax = d;
                        }
                    }
                }

                foreach (var kvp in validYHeights)
                {
                    int normalizedAngle = ((kvp.Key - ffxivStart) % 360 + 360) % 360;
                    if (normalizedAngle < bestLen && kvp.Value > arcMaxY)
                        arcMaxY = kvp.Value;
                }

                float fanHeight = 0f;
                if (arcMaxY != float.MinValue && arcMaxY > nodePos.Y)
                    fanHeight = MathF.Round((arcMaxY - nodePos.Y) + 0.2f, 2);

                route.Gathering_FanInfo.Fan_StartAngle = ffxivStart;
                route.Gathering_FanInfo.Fan_EndAngle = ffxivEnd;
                route.Gathering_FanInfo.Fan_DistanceMin = MathF.Round(allMin, 1);
                route.Gathering_FanInfo.Fan_DistanceMax = MathF.Round(allMax, 1);
                route.Gathering_FanInfo.Fan_Height = fanHeight;

                _fanGenStatus = $"Generated! Angles: {ffxivStart}→{ffxivEnd} (arc {bestLen}°), Distance: {allMin:F1}→{allMax:F1}, Height: {fanHeight:F2}";
                IceLogging.Info($"[FanGen] Node {route.Position}: FFXIV {ffxivStart}→{ffxivEnd}, dist {allMin:F1}→{allMax:F1}, height {fanHeight:F2}", tag);
            }
            catch (Exception ex)
            {
                _fanGenStatus = $"Error: {ex.Message}";
                IceLogging.Error($"[FanGen] Failed: {ex.Message}", tag);
            }
            finally
            {
                _isGeneratingFan = false;
            }
        }
        private static async Task GenerateFlightFanForNode(GatheringNode route)
        {
            _isGeneratingFan = true;
            _fanGenStatus = string.Empty;

            string tag = "Task: Generate Flight Fan";

            try
            {
                // Guard: gathering fan must exist first since we constrain to its arc
                int gatherStart = (int)route.Gathering_FanInfo.Fan_StartAngle;
                int gatherEnd = (int)route.Gathering_FanInfo.Fan_EndAngle;
                int gatherLen = ((gatherEnd - gatherStart) % 360 + 360) % 360 + 1;

                if (gatherLen == 0)
                {
                    _fanGenStatus = "Generate the gathering fan first.";
                    return;
                }

                Vector3 nodePos = route.Position;

                const float snapToleranceXZ = 0.5f;
                const float snapToleranceY = 5f;
                const float landDistanceMin = 2.5f;
                const float landDistanceMax = 4.0f;
                const float distanceStep = 0.5f;

                var validDistances = new Dictionary<int, List<float>>();
                var validYHeights = new Dictionary<int, float>();

                await Task.Run(() =>
                {
                    for (int i = 0; i < gatherLen; i++)
                    {
                        int angleDeg = (gatherStart + i) % 360;
                        bool allDistancesValid = true;
                        var distancesForAngle = new List<float>();
                        float highestY = float.MinValue;

                        for (float dist = landDistanceMin; dist <= landDistanceMax; dist += distanceStep)
                        {
                            float standardAngle = 180f - angleDeg;
                            float rad = standardAngle * (MathF.PI / 180f);
                            Vector3 candidate = new Vector3(
                                nodePos.X + dist * MathF.Sin(rad),
                                nodePos.Y,
                                nodePos.Z + dist * MathF.Cos(rad)
                            );

                            var nearest = P.navmesh.NearestPointReachable(candidate, snapToleranceXZ, snapToleranceY);
                            if (nearest.HasValue)
                            {
                                float xzDist = MathF.Sqrt(
                                    MathF.Pow(nearest.Value.X - candidate.X, 2) +
                                    MathF.Pow(nearest.Value.Z - candidate.Z, 2)
                                );
                                float yDist = MathF.Abs(nearest.Value.Y - candidate.Y);

                                if (xzDist <= snapToleranceXZ && yDist <= snapToleranceY)
                                {
                                    distancesForAngle.Add(dist);
                                    if (nearest.Value.Y > highestY)
                                        highestY = nearest.Value.Y;
                                }
                                else
                                {
                                    allDistancesValid = false;
                                    break;
                                }
                            }
                            else
                            {
                                allDistancesValid = false;
                                break;
                            }
                        }

                        if (allDistancesValid && distancesForAngle.Count > 0)
                        {
                            validDistances[angleDeg] = distancesForAngle;
                            validYHeights[angleDeg] = highestY;
                        }
                    }
                });

                if (validDistances.Count == 0)
                {
                    _fanGenStatus = "No reachable flight points found within gathering arc.";
                    return;
                }

                // Find largest contiguous sub-arc within the gathering arc bounds
                // Indexed locally (0..gatherLen-1) so no wraparound needed
                bool[] valid = new bool[gatherLen];
                foreach (var kvp in validDistances)
                {
                    int localIdx = ((kvp.Key - gatherStart) % 360 + 360) % 360;
                    if (localIdx < gatherLen)
                        valid[localIdx] = true;
                }

                int bestStart = 0, bestLen = 0;
                int currentStart = 0, currentLen = 0;

                for (int i = 0; i < gatherLen; i++)
                {
                    if (valid[i])
                    {
                        if (currentLen == 0) currentStart = i;
                        currentLen++;
                        if (currentLen > bestLen) { bestLen = currentLen; bestStart = currentStart; }
                    }
                    else
                    {
                        currentLen = 0;
                    }
                }

                if (bestLen == 0)
                {
                    _fanGenStatus = "Could not find a contiguous flight arc within gathering bounds.";
                    return;
                }

                int ffxivStart = (gatherStart + bestStart) % 360;
                int ffxivEnd = (gatherStart + bestStart + bestLen - 1) % 360;

                float allMin = float.MaxValue, allMax = float.MinValue;
                float arcMaxY = float.MinValue;

                foreach (var kvp in validDistances)
                {
                    int localIdx = ((kvp.Key - gatherStart) % 360 + 360) % 360;
                    if (localIdx >= bestStart && localIdx < bestStart + bestLen)
                    {
                        foreach (var d in kvp.Value)
                        {
                            if (d < allMin) allMin = d;
                            if (d > allMax) allMax = d;
                        }
                    }
                }

                foreach (var kvp in validYHeights)
                {
                    int localIdx = ((kvp.Key - gatherStart) % 360 + 360) % 360;
                    if (localIdx >= bestStart && localIdx < bestStart + bestLen && kvp.Value > arcMaxY)
                        arcMaxY = kvp.Value;
                }

                float fanHeight = 0f;
                if (arcMaxY != float.MinValue && arcMaxY > nodePos.Y)
                    fanHeight = MathF.Round((arcMaxY - nodePos.Y) + 0.2f, 2);

                route.Flight_FanInfo.Fan_StartAngle = ffxivStart;
                route.Flight_FanInfo.Fan_EndAngle = ffxivEnd;
                route.Flight_FanInfo.Fan_DistanceMin = MathF.Round(allMin, 1);
                route.Flight_FanInfo.Fan_DistanceMax = MathF.Round(allMax, 1);
                route.Flight_FanInfo.Fan_Height = fanHeight;

                _fanGenStatus = $"Flight fan generated! Angles: {ffxivStart}→{ffxivEnd} (arc {bestLen}°), Distance: {allMin:F1}→{allMax:F1}, Height: {fanHeight:F2}";
                IceLogging.Info($"[FanGen] Flight {route.Position}: FFXIV {ffxivStart}→{ffxivEnd}, dist {allMin:F1}→{allMax:F1}, height {fanHeight:F2}", tag);
            }
            catch (Exception ex)
            {
                _fanGenStatus = $"Error: {ex.Message}";
                IceLogging.Error($"[FanGen] Flight failed: {ex.Message}", tag);
            }
            finally
            {
                _isGeneratingFan = false;
            }
        }

        #endregion
    }
}
