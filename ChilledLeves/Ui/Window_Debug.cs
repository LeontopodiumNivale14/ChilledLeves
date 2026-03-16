using ChilledLeves.Ui.DebugTabs;
using ChilledLeves.Ui.Old_Ui;
using Dalamud.Interface.Utility.Raii;
using InteropGenerator.Runtime.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal class Window_Debug : Window
    {
        public Window_Debug() : base($"Chilled Leves: Debug [{P.GetType().Assembly.GetName().Version}] ##ChilledLevesDebugWindow")
        {
            Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            SizeConstraints = new()
            {
                MinimumSize = new(200, 200)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        private string SelectedTab = "Table: Leve Details";

        private readonly Dictionary<string, Action> DebugTabs = new()
        {
            ["Table: Leve Details"] = () => Table_LeveInfo.Draw(),
            ["Ui: PlayerInfo"] = () => Ui_PlayerInfo.Draw(),
            ["Debug: Artisan Details"] = () => Debug_ArtisanItems.CraftingDebug(),
        };

        public override void Draw()
        {
            using var colors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.Header, Theme_Colors.HeaderBg) : default;
            if (C.UseIceTheme)
            {
                colors.Push(ImGuiCol.HeaderHovered, Theme_Colors.HeaderHovered);
                colors.Push(ImGuiCol.HeaderActive, Theme_Colors.HeaderActive);
                colors.Push(ImGuiCol.Button, ThemeHelper.ButtonBg);
                colors.Push(ImGuiCol.ButtonHovered, ThemeHelper.ButtonHovered);
                colors.Push(ImGuiCol.ButtonActive, ThemeHelper.ButtonActive);
                colors.Push(ImGuiCol.WindowBg, ThemeHelper.DarkSlate);
                colors.Push(ImGuiCol.FrameBg, ThemeHelper.FrameBg);
                colors.Push(ImGuiCol.FrameBgHovered, ThemeHelper.FrameBgHovered);
                colors.Push(ImGuiCol.FrameBgActive, ThemeHelper.FrameBgActive);
                colors.Push(ImGuiCol.CheckMark, ThemeHelper.IceBlue);
            }

            using var style = C.UseIceTheme ? ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 4.0f) : default;

            float spacing = 10f;
            float leftPanelWidth = 200f;
            float rightPanelWidth = ImGui.GetContentRegionAvail().X - leftPanelWidth - spacing;
            float childHeight = ImGui.GetContentRegionAvail().Y;

            if (ImGui.BeginTable("Debug Window: Table Details", 2, ImGuiTableFlags.None, ImGui.GetContentRegionAvail()))
            {
                ImGui.TableSetupColumn("Selector", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Details", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, ThemeHelper.ChildBg) : default;
                if (ImGui.BeginChild("Debug Selector##DebugSelector_ChilledLeves", ImGui.GetContentRegionAvail(), true))
                {
                    foreach (var viewName in DebugTabs.Keys)
                    {
                        bool isSelected = (SelectedTab == viewName);
                        string label = isSelected ? $"→ {viewName}" : $"{viewName}";

                        if (ImGui.Selectable(label, isSelected))
                        {
                            SelectedTab = viewName;
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.TableNextColumn();
                if (ImGui.BeginChild("Debug Viewer Tab##DebugViewTabDetails", ImGui.GetContentRegionAvail(), true))
                {
                    if (DebugTabs.TryGetValue(SelectedTab, out var drawAction))
                        drawAction();
                    else
                        ImGui.Text("Unknown debugger tab");
                }
                ImGui.EndChild();

                ImGui.EndTable();
            }
        }
    }
}
