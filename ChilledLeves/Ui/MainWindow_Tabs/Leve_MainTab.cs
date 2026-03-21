using ChilledLeves.Enums;
using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.Game.CurrencyManager;

namespace ChilledLeves.Ui.MainWindow_Tabs;

internal class Leve_MainTab
{
    public static void Draw()
    {
        float textLineHeight = ImGui.GetTextLineHeight();
        Vector2 buttonSize = new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f);

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

        LeveClass[] Crafters = { LeveClass.Crp, LeveClass.Bsm, LeveClass.Arm, LeveClass.Gsm, LeveClass.Ltw, LeveClass.Wvr, LeveClass.Alc, LeveClass.Cul };
        LeveClass[] Gatherers = { LeveClass.Min, LeveClass.Btn, LeveClass.Fsh };

        // Crafters section
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);
        Theme_Colors.BodyText("Crafters");
        var currentItem = 0;

        foreach (var job in Crafters)
        {
            if (currentItem % itemsPerRow == 0)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);

            JobToggleButton(job);
            currentItem++;

            if (currentItem % itemsPerRow != 0 && job != Crafters[^1])
                ImGui.SameLine(0, iconSpacing);
        }

        // Gatherers section
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);
        Theme_Colors.BodyText("Gatherers");
        currentItem = 0;

        foreach (var job in Gatherers)
        {
            if (currentItem % itemsPerRow == 0)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);

            JobToggleButton(job);
            currentItem++;

            if (currentItem % itemsPerRow != 0 && job != Gatherers[^1])
                ImGui.SameLine(0, iconSpacing);
        }

        ImGui.Dummy(new Vector2(0, 5));

        Theme_Colors.HeaderText("Additional Filters");
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 5));

        if (ImGui.BeginTable("Filter Settings", 2, ImGuiTableFlags.SizingFixedFit, ImGui.GetContentRegionAvail()))
        {
            ImGui.TableSetupColumn("Type");
            ImGui.TableSetupColumn("Info", ImGuiTableColumnFlags.WidthStretch);

            var minLv = C.MinLevel;
            var maxLv = C.MaxLevel;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            Theme_Colors.BodyText($"Min Level");

            ImGui.TableNextColumn();
            if (ImGui.SliderInt("##Min Lv.", ref minLv, 1, maxLv))
            {
                C.MinLevel = minLv;
                C.SaveDebounced();
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            Theme_Colors.BodyText($"Max Lv.");

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            if (ImGui.SliderInt("##Max Level", ref maxLv, minLv, 100))
            {
                C.MaxLevel = maxLv;
                C.SaveDebounced();
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            Theme_Colors.BodyText("Leve Name");

            ImGui.TableNextColumn();
            ImGui.InputText("##Name Search", ref Leve_SelectionTab.Leve_Name, 1000);

            ImGui.EndTable();
        }
    }

    private static void JobToggleButton(LeveClass selectedClass)
    {
        bool enabled = C.Job_LeveFilter[selectedClass];

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
            var image = LeveInfo.Job_IconDict[selectedClass].ColorIcon;
            if (ImGui.ImageButton(image.GetWrapOrEmpty().Handle, size))
            {
                C.Job_LeveFilter[selectedClass] = !C.Job_LeveFilter[selectedClass];
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                foreach (var jobName in C.Job_LeveFilter)
                {
                    if (jobName.Key != selectedClass)
                        C.Job_LeveFilter[jobName.Key] = false;
                    else
                        C.Job_LeveFilter[jobName.Key] = true;
                }
                C.Save();
            }
        }
        else
        {
            var image = LeveInfo.GreyJobIcon(selectedClass);
            if (ImGui.ImageButton(image.Handle, size))
            {
                C.Job_LeveFilter[selectedClass] = !C.Job_LeveFilter[selectedClass];
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                foreach (var jobName in C.Job_LeveFilter)
                {
                    if (jobName.Key != selectedClass)
                        C.Job_LeveFilter[jobName.Key] = false;
                    else
                        C.Job_LeveFilter[jobName.Key] = true;
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

    public static string GetJobName(LeveClass @class)
    {
        return @class switch
        {
            LeveClass.Crp => "Carpenter",
            LeveClass.Bsm => "Blacksmith",
            LeveClass.Arm => "Armorer",
            LeveClass.Gsm => "Goldsmith",
            LeveClass.Ltw => "Leatherworker",
            LeveClass.Wvr => "Weaver",
            LeveClass.Alc => "Alchemist",
            LeveClass.Cul => "Culinarian",
            LeveClass.Min => "Miner",
            LeveClass.Btn => "Botanist",
            LeveClass.Fsh => "Fisher",
            _ => throw new ArgumentException($"Unknown job ID: {@class}")
        };
    }
}
