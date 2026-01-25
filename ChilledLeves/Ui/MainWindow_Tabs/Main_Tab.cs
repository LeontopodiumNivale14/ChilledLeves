using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs;

internal class Main_Tab
{
    public static void Draw()
    {
        float textLineHeight = ImGui.GetTextLineHeight();
        Vector2 buttonSize = new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f);

        if (ImGui.Button("Start", buttonSize))
        {

        }

        if (ImGui.Button("Stop", buttonSize))
        {

        }

        if (ImGui.Button("Open Worklist", buttonSize))
        {

        }

        if (ImGui.Button("Open Priority Grind", buttonSize))
        {

        }

        ImGui.Dummy(new(0, 5));

        bool useCustomTheme = C.UseIceTheme;
        if (ImGui.Checkbox("Use Ice Theme", ref useCustomTheme))
        {
            C.UseIceTheme = useCustomTheme;
            C.Save();
        }
        ImGui.SameLine();
        ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Toggle on/off your basic theme and a fancy Ice Color theme ~");
        }
        ImGui.NewLine();

        bool rapidImport = C.RapidImport;
        if (ImGui.Checkbox("Rapid Import Leves", ref rapidImport))
        {
            C.RapidImport = rapidImport;
            C.Save();
        }
        ImGui.SameLine();
        ImGuiEx.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.Text("When you left click a leve, will immediately add it to your worklist");
            ImGui.Text("While this is disabled, you can also double click to add a leve as well!");
            ImGui.EndTooltip();
        }
        ImGui.NewLine();

        if(ImGui.Button("Alert Settings", buttonSize))
        {

        }
        ImGui.Dummy(new Vector2(0, 5));

        Theme_Colors.HeaderText("Filter Options");
        ImGui.Separator();

        ImGui.Dummy(new Vector2(0, 5));
        bool favorites = C.Leve_Filter["Favorites"];
        if (ImGui.Checkbox("Show Favorites Only", ref favorites))
        {
            foreach (var filter in C.Leve_Filter)
            {
                C.Leve_Filter[filter.Key] = false;
            }
            C.Leve_Filter["Favorites"] = favorites;
            C.Save();
        }

        bool showCompleted = C.Leve_Filter["Completed"];
        if (ImGui.Checkbox("Show Completed Only", ref showCompleted))
        {
            foreach (var filter in C.Leve_Filter)
            {
                C.Leve_Filter[filter.Key] = false;
            }
            C.Leve_Filter["Completed"] = showCompleted;
            C.Save();
        }

        bool showInCompleted = C.Leve_Filter["Incomplete"];
        if (ImGui.Checkbox("Show Incomplete Only", ref showInCompleted))
        {
            foreach (var filter in C.Leve_Filter)
            {
                C.Leve_Filter[filter.Key] = false;
            }
            C.Leve_Filter["Incomplete"] = showInCompleted;
            C.Save();
        }

        ImGui.Dummy(new(0, 5));

        float iconSpacing = 6;

        int itemsPerRow = 4;
        int currentItem = 0;

        // Crafters section
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);
        Theme_Colors.BodyText("Crafters");

        for (uint i = 8; i < 16; i++)
        {
            if (currentItem % itemsPerRow == 0)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);

            JobToggleButton(i);
            currentItem++;

            if (currentItem % itemsPerRow != 0 && i != 15)
                ImGui.SameLine(0, iconSpacing);
        }

        currentItem = 0;

        // Gatherers section
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);
        Theme_Colors.BodyText("Gatherers");
        for (uint i = 16; i < 19; i++)
        {
            if (currentItem % itemsPerRow == 0)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);

            JobToggleButton(i);
            currentItem++;

            if (currentItem % itemsPerRow != 0 && i != 18)
                ImGui.SameLine(0, iconSpacing);
        }

        ImGui.Dummy(new Vector2(0, 5));

        Theme_Colors.HeaderText("Additional Filters");
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        ImGui.Text("Min Lv.");
        ImGui.SameLine();
        ImGui.SliderInt("##Min Level", ref Leve_Tab.Leve_MinLevel, 1, Leve_Tab.Leve_MaxLevel);

        ImGui.Text("Max Lv.");
        ImGui.SameLine();
        ImGui.SliderInt("##Max Level", ref Leve_Tab.Leve_MaxLevel, Leve_Tab.Leve_MinLevel, 100);
        ImGui.Text($"Leve Name");
        ImGui.SameLine();
        ImGui.InputText("##Name Search", ref Leve_Tab.Leve_Name, 1000);
    }

    private static void JobToggleButton(uint jobId)
    {
        var jobButton_Name = GetJobName(jobId);
        bool enabled = C.Job_Filter[jobButton_Name];

        using var framePadding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
        using var colors = ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero); // Initialize with dummy
        using var styles = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 2.0f);

        if (enabled)
        {
            // Indicate an enabled job filter with some highlight
            if (C.UseIceTheme)
            {
                colors.Push(ImGuiCol.Button, Theme_Colors.LightSlate);
                colors.Push(ImGuiCol.Border, Theme_Colors.IceBlue);
            }
            else
            {
                colors.Push(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.35f, 0.7f));
                colors.Push(ImGuiCol.Border, ImGuiColors.ParsedGold);
            }
            styles.Push(ImGuiStyleVar.FrameBorderSize, 1.0f);
        }
        else if (C.UseIceTheme)
        {
            // Disabled job with Ice theme
            colors.Push(ImGuiCol.Button, Theme_Colors.DarkSlate);
            colors.Push(ImGuiCol.Border, Theme_Colors.TranslucentIce);
            styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
        }
        else
        {
            // Disabled job with Dalamud theme
            colors.Push(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 0.1f));
            colors.Push(ImGuiCol.Border, new Vector4(0.4f, 0.4f, 0.4f, 0.5f));
            styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
        }

        var globalScale = ImGuiHelpers.GlobalScale;
        Vector2 size = new Vector2(26 * globalScale, 26 * globalScale);

        if (enabled)
        {
            var image = LeveInfo.Job_IconDict[jobId].ColorIcon;
            if (ImGui.ImageButton(image.GetWrapOrEmpty().Handle, size))
            {
                C.Job_Filter[jobButton_Name] = !C.Job_Filter[jobButton_Name];
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                foreach (var jobName in C.Job_Filter)
                {
                    if (jobName.Key != jobButton_Name)
                        C.Job_Filter[jobName.Key] = false;
                    else
                        C.Job_Filter[jobName.Key] = true;
                }
                C.Save();
            }
        }
        else
        {
            var image = LeveInfo.GreyJobIcon(jobId);
            if (ImGui.ImageButton(image.Handle, size))
            {
                C.Job_Filter[jobButton_Name] = !C.Job_Filter[jobButton_Name];
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                foreach (var jobName in C.Job_Filter)
                {
                    if (jobName.Key != jobButton_Name)
                        C.Job_Filter[jobName.Key] = false;
                    else
                        C.Job_Filter[jobName.Key] = true;
                }
                C.Save();
            }
        }
    }

    public static uint GetJobId(string jobName)
    {
        return jobName switch
        {
            "Carpenter" => 8,
            "Blacksmith" => 9,
            "Armorer" => 10,
            "Goldsmith" => 11,
            "Leatherworker" => 12,
            "Weaver" => 13,
            "Alchemist" => 14,
            "Culinarian" => 15,
            "Miner" => 16,
            "Botanist" => 17,
            "Fisher" => 18,
            _ => throw new ArgumentException($"Unknown job: {jobName}")
        };
    }

    public static string GetJobName(uint jobId)
    {
        return jobId switch
        {
            8 => "Carpenter",
            9 => "Blacksmith",
            10 => "Armorer",
            11 => "Goldsmith",
            12 => "Leatherworker",
            13 => "Weaver",
            14 => "Alchemist",
            15 => "Culinarian",
            16 => "Miner",
            17 => "Botanist",
            18 => "Fisher",
            _ => throw new ArgumentException($"Unknown job ID: {jobId}")
        };
    }
}
