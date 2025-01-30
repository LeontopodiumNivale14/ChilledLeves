using Dalamud.Interface.Components;
using ExplorersIcebox.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui.MainWindow.GrindModeUi;

internal static class MaximizeStock
{
    private static string SearchQuery = ""; // private static variable to store the search query
    private static int UpdateAllWS = 0;

    private static void UpdateWSImGui()
    {
    }

    internal static void Draw()
    {
        ImGui.SetNextItemWidth(300);
        ImGui.InputText("Search", ref SearchQuery, 100);
        if (SchedulerMain.WorkshopSelected == false)
            ImGui.TextWrapped("Select how much you would like to gather from the routes");
        else if (SchedulerMain.WorkshopSelected == true)
            ImGui.TextWrapped("Select which routes you would like to cap items from:");
        ImGui.Spacing();

        if (ImGui.Button("Enable All Routes"))
        {
            RouteDataPoint[0].GatherRoute = true;
            RouteDataPoint[1].GatherRoute = true;
            RouteDataPoint[2].GatherRoute = true;
            RouteDataPoint[3].GatherRoute = true;
            RouteDataPoint[4].GatherRoute = true;
            RouteDataPoint[5].GatherRoute = true;
            RouteDataPoint[6].GatherRoute = true;
            RouteDataPoint[7].GatherRoute = true;
            RouteDataPoint[8].GatherRoute = true;
            RouteDataPoint[9].GatherRoute = true;
            RouteDataPoint[10].GatherRoute = true;
            RouteDataPoint[11].GatherRoute = true;
            RouteDataPoint[12].GatherRoute = true;
            RouteDataPoint[13].GatherRoute = true;
            RouteDataPoint[14].GatherRoute = true;
            RouteDataPoint[15].GatherRoute = true;
            RouteDataPoint[16].GatherRoute = true;
            RouteDataPoint[17].GatherRoute = true;
            RouteDataPoint[18].GatherRoute = true;
            RouteDataPoint[19].GatherRoute = true;
            RouteDataPoint[20].GatherRoute = true;
            RouteDataPoint[21].GatherRoute = true;
            PluginLog("Enabled all routes");
        }
        ImGui.SameLine();
        if (ImGui.Button("Disable All Routes"))
        {
            RouteDataPoint[0].GatherRoute = false;
            RouteDataPoint[1].GatherRoute = false;
            RouteDataPoint[2].GatherRoute = false;
            RouteDataPoint[3].GatherRoute = false;
            RouteDataPoint[4].GatherRoute = false;
            RouteDataPoint[5].GatherRoute = false;
            RouteDataPoint[6].GatherRoute = false;
            RouteDataPoint[7].GatherRoute = false;
            RouteDataPoint[8].GatherRoute = false;
            RouteDataPoint[9].GatherRoute = false;
            RouteDataPoint[10].GatherRoute = false;
            RouteDataPoint[11].GatherRoute = false;
            RouteDataPoint[12].GatherRoute = false;
            RouteDataPoint[13].GatherRoute = false;
            RouteDataPoint[14].GatherRoute = false;
            RouteDataPoint[15].GatherRoute = false;
            RouteDataPoint[16].GatherRoute = false;
            RouteDataPoint[17].GatherRoute = false;
            RouteDataPoint[18].GatherRoute = false;
            RouteDataPoint[19].GatherRoute = false;
            RouteDataPoint[20].GatherRoute = false;
            RouteDataPoint[21].GatherRoute = false;
            PluginLog("Disabled all routes");

        }
        UpdateWSImGui();
        if (SchedulerMain.WorkshopSelected == true)
        {
            var updateAll = UpdateAllWS;
            ImGui.SameLine();
            ImGui.SetCursorPosX(offSet(120f));
            ImGui.SetNextItemWidth(85);
            if (ImGui.InputInt("##Update All Workshops", ref updateAll))
            {
                UpdateAllWS = AmountSet(updateAll);
                {
                    foreach (var item in IslandItemDict.Keys.ToList())
                    {
                        IslandItemDict[item].Workshop = AmountSet(updateAll);
                    }
                }
            }
            ImGuiComponents.HelpMarker("Quick way to update all workshops to the same value.");
        }

        #region SearchQuery
        bool ViewWorkshop = SchedulerMain.WorkshopSelected;

        if (string.IsNullOrEmpty(SearchQuery) || "Islefish | Clam [Route 0]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Islefish | Clam [Route 0]"))
            {
                ImGuiComponents.HelpMarker("Flying is required, due to how going under water works...");
                SharedWorkshopUI.RouteUi(0, RouteAmount(0, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[0].GatherRoute);
                ImGui.TreePop();
            }
            ImGuiComponents.HelpMarker("Flying is required, due to how going under water works...");
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Islewort [Route 1]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Islewort [Route 1]"))
            {
                SharedWorkshopUI.RouteUi(1, RouteAmount(1, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[1].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Sugarcane | Vine [Route 2]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Sugarcane | Vine [Route 2]"))
            {
                SharedWorkshopUI.RouteUi(2, RouteAmount(2, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[2].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Tinsand | Sand [Route 3]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Tinsand | Sand [Route 3]"))
            {
                SharedWorkshopUI.RouteUi(3, RouteAmount(3, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[3].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Apple | Beehive | Vine [Route 4]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Apple | Beehive | Vine [Route 4]"))
            {
                SharedWorkshopUI.RouteUi(4, RouteAmount(4, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[4].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Coconut | Palm Log | Palm leaf [Route 5]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Coconut | Palm Log | Palm leaf [Route 5]"))
            {
                SharedWorkshopUI.RouteUi(5, RouteAmount(5, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[5].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Cotton [Route 6]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Cotton [Route 6]"))
            {
                SharedWorkshopUI.RouteUi(6, RouteAmount(6, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[6].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Clay | Sand [Route 7]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Clay | Sand [Route 7]"))
            {
                SharedWorkshopUI.RouteUi(7, RouteAmount(7, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[7].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Marble | Limestone | Stone [Route 8]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Marble | Limestone | Stone [Route 8]"))
            {
                SharedWorkshopUI.RouteUi(8, RouteAmount(8, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[8].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Branch | Resin | Log [Route 9]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Branch | Resin | Log [Route 9]"))
            {
                SharedWorkshopUI.RouteUi(9, RouteAmount(9, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[9].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Copper | Mythril [Route 10]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Copper | Mythril [Route 10]"))
            {
                SharedWorkshopUI.RouteUi(10, RouteAmount(10, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[10].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Opal | Sap | (Log) [Route 11]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Opal | Sap | (Log) [Route 11]"))
            {
                SharedWorkshopUI.RouteUi(11, RouteAmount(11, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[11].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Hemp | (Islewort) [Route 12]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Hemp | (Islewort) [Route 12]"))
            {
                SharedWorkshopUI.RouteUi(12, RouteAmount(12, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[12].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Multicolorblooms [Route 13]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Multicolorblooms [Route 13]"))
            {
                ImGuiComponents.HelpMarker("Flying required");
                SharedWorkshopUI.RouteUi(13, RouteAmount(13, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[13].GatherRoute);
                ImGui.TreePop();
            }
            ImGuiComponents.HelpMarker("Flying required");
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Iron Ore [Route 14]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Iron Ore [Route 14]"))
            {
                SharedWorkshopUI.RouteUi(14, RouteAmount(14, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[14].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Laver | Squid / Jellyfish | Coral [Route 15]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Laver | Squid / Jellyfish | Coral [Route 15]"))
            {
                ImGuiComponents.HelpMarker("Flying is required, due to how going under water works...");
                SharedWorkshopUI.RouteUi(15, RouteAmount(15, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[15].GatherRoute);
                ImGui.TreePop();
            }
            ImGuiComponents.HelpMarker("Flying is required, due to how going under water works...");
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Rocksalt [Route 16]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Rocksalt [Route 16]"))
            {
                SharedWorkshopUI.RouteUi(16, RouteAmount(16, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[16].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Leucogranite [Route 17}".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Leucogranite [Route 17}"))
            {
                SharedWorkshopUI.RouteUi(17, RouteAmount(17, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[17].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Quartz | Stone [Route 18]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Quartz | Stone [Route 18]"))
            {
                ImGuiComponents.HelpMarker("Flying is required");
                SharedWorkshopUI.RouteUi(18, RouteAmount(18, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[18].GatherRoute);
                ImGui.TreePop();
            }
            ImGuiComponents.HelpMarker("Flying is required");
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Glimshroom | Shale / Coal [Route 19]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Glimshroom | Shale / Coal [Route 19]"))
            {
                SharedWorkshopUI.RouteUi(19, RouteAmount(19, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[19].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Effervescent Water / Spectrine [Route 20]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {

            if (ImGui.TreeNode("Effervescent Water / Spectrine [Route 20]"))
            {
                SharedWorkshopUI.RouteUi(20, RouteAmount(20, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[20].GatherRoute);
                ImGui.TreePop();
            }
        }
        if (string.IsNullOrEmpty(SearchQuery) || "Yellow Copper | Gold | Crystal Formation | HawksEyeSand [Route 21]".Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        {
            if (ImGui.TreeNode("Yellow Copper | Gold | Crystal Formation | HawksEyeSand [Route 21]"))
            {
                SharedWorkshopUI.RouteUi(21, RouteAmount(21, ViewWorkshop), ViewWorkshop, true, RouteDataPoint[21].GatherRoute);
                ImGui.TreePop();
            }
        }
        #endregion
    }
}
