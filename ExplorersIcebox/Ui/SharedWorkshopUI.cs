using Dalamud.Interface.Components;
using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui;

internal static class SharedWorkshopUI
{
    private static int AmountSet(int input)
    {
        if (input < 0) input = 0;
        else if (input > 999) input = 999;
        EzConfig.Save();
        UpdateTableDict();
        return input;
    }

    private static float offSet(float value)
    {
        var windowWidth = ImGui.GetWindowContentRegionMax().X; // Get the usable width of the window
        var inputWidth = value; // Desired width of the input field
        var offset = windowWidth - inputWidth; // Calculate position to hug the right wall
        return offset;
    }

    private static void BaseRouteTable(string routeTableName, int RouteAmount, bool Workshop, List<RouteEntry> RouteTable, int RouteNumber)
    {
        string column4th = "";
        string amountGathered = string.Empty;
        string itemName = string.Empty;
        if (ImGui.BeginTable(routeTableName, 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Currently Have", ImGuiTableColumnFlags.WidthFixed, 100);
            ImGui.TableSetupColumn("Items Per Loop", ImGuiTableColumnFlags.WidthFixed, 110);
            if (Workshop)
            {
                column4th = "Amount to Keep";
                ImGui.TableSetupColumn(column4th);
            }
            else if (!Workshop)
            {
                column4th = "Amount to Gather";
                ImGui.TableSetupColumn(column4th);
            }
            ImGui.TableSetupColumn("Total Gather Amount");

            ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

            string[] headers = { "Item", "Currently Have", "Amount Per Loop", column4th, "Total Gather Amount" };
            for (int col = 0; col < headers.Length; col++)
            {
                ImGui.TableSetColumnIndex(col);

                // Calculate the available space and text size
                var header = headers[col];
                var textSize = ImGui.CalcTextSize(header);
                var columnWidth = ImGui.GetColumnWidth();
                var cursorPosX = ImGui.GetCursorPosX();

                // Set the cursor position to center the text
                ImGui.SetCursorPosX(cursorPosX + (columnWidth - textSize.X) / 2.0f);
                ImGui.Text(header);
            }

            for (int i = 0; i < RouteTable.Count; i++)
            {
                int itemID = RouteTable[i].ID;
                int amountPerLoop = RouteTable[i].AmountGatherable;
                int AmountGatheredTotal = (RouteAmount * amountPerLoop);
                if (AmountGatheredTotal > 999)
                {
                    amountGathered = "999+";
                }
                else
                {
                    amountGathered = $"{AmountGatheredTotal}";
                }
                int MaxGatherAmount = (RouteDataPoint[RouteNumber].MaxLoops * amountPerLoop);
                if (MaxGatherAmount > 999)
                    MaxGatherAmount = 999;
                int WorkshopInput = IslandItemDict[itemID].Workshop;
                int GatherInput = RouteTable[i].GatherAmount;
                bool CanIgnore = RouteTable[i].CanSellFullAmount;

                string[] rowValues =
                [
                $"{IslandItemDict[itemID].Name}",
                $"{GetItemCount(itemID)}",
                $"{RouteTable[i].AmountGatherable}",
                "Dummy",
                $"{amountGathered}"
                ];

                ImGui.TableNextRow();
                for (int col = 0; col < 5; col++)
                {
                    ImGui.TableSetColumnIndex(col);

                    if (col < 3 || col == 4) // For the first four columns
                    {
                        var text = rowValues[col];
                        var textSize = ImGui.CalcTextSize(text);
                        var columnWidth = ImGui.GetColumnWidth();
                        var cursorPosX = ImGui.GetCursorPosX();

                        ImGui.SetCursorPosX(cursorPosX + (columnWidth - textSize.X) / 2.0f);
                        ImGui.Text(text);
                        if (col == 0)
                        {
                            if (CanIgnore == true)
                            {
                                ImGui.SetCursorPosX(cursorPosX + (columnWidth - textSize.X) / 2.0f + 5);
                                ImGuiComponents.HelpMarker("Item not factored into how many loops it can do.");
                            }
                        }
                    }
                    else if (col == 3) // For the 5th column with ImGui.InputInt
                    {
                        var inputLabel = $"##ItemImGui_{IslandItemDict[itemID].Name}";
                        ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                        if (Workshop)
                        {
                            if (ImGui.SliderInt(inputLabel, ref WorkshopInput, 0, 999))
                            {
                                WorkshopInput = AmountSet(WorkshopInput);
                                IslandItemDict[itemID].Workshop = WorkshopInput;
                            }
                        }
                        else if (!Workshop)
                        {

                            if (ImGui.SliderInt(inputLabel, ref GatherInput, 0, MaxGatherAmount))
                            {
                                GatherInput = AmountSet(GatherInput);
                                if (GatherInput > MaxGatherAmount)
                                    GatherInput = MaxGatherAmount;
                                if (GatherInput < 0)
                                    GatherInput = 0;
                                RouteTable[i].GatherAmount = GatherInput;
                            }
                        }
                    }
                }
            }
        }
        ImGui.EndTable();
    }

    internal static void RouteUi(int RouteNumber, int RouteAmount, bool Workshop, bool ShowEnable, bool ConfigRoute)
    {
        string tableName = $"Route {RouteNumber}";
        if (ImGui.BeginTable($"{tableName}_Ui", 2))
        {
            ImGui.TableSetupColumn("Route Info", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Checkbox");
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Route {RouteNumber} is set to run → {RouteAmount}");
            ImGui.TableSetColumnIndex(1);
            if (ShowEnable)
            {
                ImGui.Text("Enable Route");
                ImGui.SameLine();
                if (ImGui.Checkbox($"##EnableRoute{RouteNumber}", ref ConfigRoute))
                {
                    if (ConfigRoute)
                    {
                        ConfigRoute = true;
                        RouteDataPoint[RouteNumber].GatherRoute = true;
                    }
                    else
                    {
                        ConfigRoute = false;
                        RouteDataPoint[RouteNumber].GatherRoute = false;
                    }
                }
            }
        }

        ImGui.EndTable();

        BaseRouteTable(tableName, RouteAmount, Workshop, GetTable(RouteNumber), RouteNumber);
    }
}
