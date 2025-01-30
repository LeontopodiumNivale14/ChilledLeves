using Dalamud.Interface.Utility.Raii;
using ExplorersIcebox.Scheduler.Tasks;
using ExplorersIcebox.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Interface.Components;

namespace ExplorersIcebox.Ui.MainWindow.GrindModeUi;

internal static unsafe class GrindModeUi
{
    private static string[] ModeSelect = { "XP | Cowries Grind", "Island Gathering Mode", "Max Island Inventory" };
    private static string[] ModeTooltips =
    {
        "Focus on grinding XP and collecting Cowries.",
        "Select how many items you want to gather per route.",
        "Maximize your island inventory space for optimal use."
    };
    private static string CurrentMode = "XP | Cowries Grind";

    internal static void Draw()
    {
        UpdateXPTable();
        ImGui.Text($"Current task → {displayCurrentTask}");
        ImGui.SameLine();
        string buttonText = P.taskManager.IsBusy ? "Stop" : "Teleport to Island Entrance";

        // Calculate text size and button size
        Vector2 textSize = ImGui.CalcTextSize(buttonText);
        Vector2 buttonSize = new Vector2(textSize.X + ImGui.GetStyle().FramePadding.X * 2, textSize.Y + ImGui.GetStyle().FramePadding.Y * 2);

        // Get available content width
        float windowWidth = ImGui.GetContentRegionAvail().X;

        // Calculate button position for right alignment
        float buttonPosX = windowWidth - buttonSize.X + 105;

        // Set cursor position to align the button
        ImGui.SetCursorPosX(buttonPosX);

        if (ImGui.Button(buttonText))
        {
            if (P.taskManager.IsBusy)
            {
                SchedulerMain.DisablePlugin();
            }
            else
            {
                TaskReturnToIsland.Enqueue();
            }
        }
        ImGui.Text($"Route → {displayCurrentRoute}");
        if (CurrentTerritory() == 1055 && IsAddonActive("MJIHud"))
        {
            string nodeText = GetNodeText("MJIHud", 14);
            int nodeValue = int.Parse(nodeText);
            if (nodeValue != IslandLevel)
            {
                IslandLevel = nodeValue;
            }
        }
        ImGui.Text($"Current level is: {IslandLevel}");
        var isXPRunning = SchedulerMain.AreWeTicking;
        using (ImRaii.Disabled(!IsInZone(IslandSancZoneID)))
        {
            if (ImGui.Checkbox(" Enable Farm", ref isXPRunning))
            {
                if (isXPRunning)
                {
                    PluginLog("Enabling the route Gathering");
                    if (CurrentMode == "Max Island Inventory" || CurrentMode == "Island Gathering Mode") C.XPGrind = false;
                    if (CurrentMode == "XP | Cowries Grind") C.XPGrind = true;
                    SchedulerMain.EnablePlugin();
                    displayCurrentTask = "Currently Running";
                }
                else
                {
                    PluginLog("Disabling the Route gathering");
                    displayCurrentTask = "";
                    SchedulerMain.DisablePlugin();
                }
            }
        }
        ImGui.SetNextItemWidth(175);
        if (ImGui.BeginCombo("##Mode Select Combo", CurrentMode))
        {
            for (int i = 0; i < ModeSelect.Length; i++)
            {
                var option = ModeSelect[i];

                // Render the selectable option
                if (ImGui.Selectable(option, option == CurrentMode))
                {
                    CurrentMode = option;
                }

                // Set focus to the selected item for better UX
                if (option == CurrentMode)
                {
                    ImGui.SetItemDefaultFocus();
                }

                // Display tooltip if this option is hovered
            }
            ImGui.EndCombo();
        }
        ImGui.Spacing();
        if (CurrentMode == "XP | Cowries Grind")
        {
            if (C.routeSelected != 7 && C.routeSelected != 18)
            {
                if (IslandLevel < 10)
                    C.routeSelected = 7;
                else if (IslandLevel >= 10)
                    C.routeSelected = 18;
                else
                    C.routeSelected = 7;
            }
            GrindXP.Draw();
        }
        else if (CurrentMode == "Island Gathering Mode")
        {
            SchedulerMain.WorkshopSelected = false;
            MaximizeStock.Draw();
        }
        else if (CurrentMode == "Max Island Inventory")
        {
            SchedulerMain.WorkshopSelected = true;
            MaximizeStock.Draw();
        }
    }
}
