using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;

namespace ChilledLeves.Ui.MainWindow_Tabs.Leve_Info;

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

        AssignmentType[] Crafters = 
        { 
            AssignmentType.Carpenter, AssignmentType.Blacksmith,
            AssignmentType.Armorer, AssignmentType.Goldsmith,
            AssignmentType.Leatherworker, AssignmentType.Weaver,
            AssignmentType.Alchemist, AssignmentType.Culinarian,  
        };
        AssignmentType[] Gatherers = 
        {
            AssignmentType.Miner, AssignmentType.Botanist, AssignmentType.Fisher
        };

        // Crafters section
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);
        ImGui.Text("Crafters");
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
        ImGui.Text("Gatherers");
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
            ImGui.Text($"Min Lv.");

            ImGui.TableNextColumn();
            if (ImGui.SliderInt("##Min Lv.", ref minLv, 1, maxLv))
            {
                C.MinLevel = minLv;
                C.SaveDebounced();
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Max Lv.");

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            if (ImGui.SliderInt("##Max Lv", ref maxLv, minLv, 100))
            {
                C.MaxLevel = maxLv;
                C.SaveDebounced();
            }

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Leve Name");

            ImGui.TableNextColumn();
            ImGui.InputTextWithHint("##Name Search", "Leve name", ref Leve_SelectionTab.Leve_Name, 1000);

            ImGui.EndTable();
        }
    }

    private static void JobToggleButton(AssignmentType selectedClass)
    {
        bool enabled = C.Assignemnt_Filter[selectedClass];

        using var framePadding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
        using var colors = ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero); // Initialize with dummy
        using var styles = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 2.0f);

        if (enabled)
        {
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
            colors.Push(ImGuiCol.Button, Theme_Colors.DarkSlate);
            colors.Push(ImGuiCol.Border, Theme_Colors.TranslucentIce);
            styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
        }
        else
        {
            colors.Push(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 0.1f));
            colors.Push(ImGuiCol.Border, new Vector4(0.4f, 0.4f, 0.4f, 0.5f));
            styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
        }

        var globalScale = ImGuiHelpers.GlobalScale;
        var size = new Vector2(28 * globalScale, 28 * globalScale);
        var iconId = LeveInfo.Assignment_IconDict[selectedClass].IconId;

        bool clicked = GameIcons.DrawButton(iconId, $"jobtoggle_{selectedClass}", size, grey: !enabled);

        if (clicked)
        {
            C.Assignemnt_Filter[selectedClass] = !C.Assignemnt_Filter[selectedClass];
        }
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            foreach (var jobName in C.Assignemnt_Filter.Keys.ToList())
                C.Assignemnt_Filter[jobName] = jobName == selectedClass;
            C.Save();
        }
    }
}
