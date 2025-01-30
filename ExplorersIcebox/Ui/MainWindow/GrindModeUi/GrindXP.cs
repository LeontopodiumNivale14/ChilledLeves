using Dalamud.Interface.Components;
using ECommons.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Ui.MainWindow.GrindModeUi;

internal class GrindXP
{
    private static string[] RunOptions = { "Infinite", "x Times" }; // Dropdown options
    private static string RunOptionsSelected = "Infinite";

    internal static void Draw()
    {
        ImGui.Text("XP | Cowries Grind");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Run Setting:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(125);
        if (ImGui.BeginCombo("##RunSettings", RunOptionsSelected))
        {
            foreach (var option in RunOptions)
            {
                if (ImGui.Selectable(option, option == RunOptionsSelected))
                {
                    RunOptionsSelected = option;
                }
                if (option == RunOptionsSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
        if (RunOptionsSelected == RunOptions[0])
        {
            C.runInfinite = true;
            EzConfig.Save();
        }
        else if (RunOptionsSelected == RunOptions[1])
        {
            C.runInfinite = false;
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("##RunAmount", ref C.runAmount);
        }
        ImGui.Spacing();
        if (ImGui.RadioButton("Ground Loop ", C.routeSelected == 7))
        {
            C.routeSelected = 7; // Sets the option to 8 (Option #1)
            PluginLog("Changed the selected route to Clay/Sand [Ground XP Loop]");
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Best to use from lv. 5 → Lv. 10 \nThe slower of the two options, but doesn't require flying to be unlocked");
        if (ImGui.RadioButton("Flying/Fast XP Route", C.routeSelected == 18))
        {
            C.routeSelected = 18; // Sets the option to 18 (Option #2)
            PluginLog("Changed the selected route to Quartz [Mountain XP Loop]");
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Faster leveling route. Best use from Lv. 10+ \nFlying is REQUIRED to do this one.");

        ImGui.Text("Workshop Items");
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("Input the amount of items that you want to keep from this loop.\nThis will automatically adjust how many loops of this route it can do.");

        if (C.routeSelected == 7)
        {
            SharedWorkshopUI.RouteUi(7, Route7MaxAmount, true, false, RouteDataPoint[7].GatherRoute);
        }
        else if (C.routeSelected == 18)
        {
            SharedWorkshopUI.RouteUi(18, Route18MaxAmount, true, false, RouteDataPoint[18].GatherRoute);
        }
    }
}
