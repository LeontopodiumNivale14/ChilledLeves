using ChilledLeves.Ui.MainWindow_Tabs;
using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal class Window_Main : Window
    {
        public Window_Main() : base($"Chilled Leves [{P.GetType().Assembly.GetName().Version}] ##ChilledLevesMainWindowV2")
        {
            Flags = ImGuiWindowFlags.None;
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

            string allowance = $"Allowances: {Utils.Allowances}/100 | Next in: {Utils.NextAllowances:hh':'mm':'ss}";
            Theme_Colors.CustomHeader(allowance, ImGui.GetContentRegionAvail().X);

            var globalScale = ImGuiHelpers.GlobalScale;

            // Wrap the table in a child that takes remaining space
            using (var tableContainer = ImRaii.Child("TableContainer", new Vector2(-1, -1), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, ThemeHelper.ChildBg) : default;

                if (ImGui.BeginTabBar("Chilled Leves: Tab Bar"))
                {
                    if (ImGui.BeginTabItem("Leve Selection"))
                    {
                        if (ImGui.BeginTable("Main Window_Headers", 3, ImGuiTableFlags.None))
                        {
                            ImGui.TableSetupColumn("Settings", ImGuiTableColumnFlags.WidthFixed, 250 * globalScale);
                            ImGui.TableSetupColumn("Mission Selection", ImGuiTableColumnFlags.WidthFixed, 500 * globalScale);
                            ImGui.TableSetupColumn("Details", ImGuiTableColumnFlags.WidthStretch);

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            Theme_Colors.HeaderText("Main Settings");
                            using (var settings_Child = ImRaii.Child("Setting Child Window", new Vector2(-1, -1), true))
                            {
                                Leve_MainTab.Draw();
                            }

                            ImGui.TableNextColumn();
                            Theme_Colors.HeaderText($"Showing {Leve_SelectionTab.LeveCount} / {Leve_SelectionTab.Leve_Total}");
                            using (var settings_Child = ImRaii.Child("Leve Details Window", new Vector2(-1, -1), true))
                            {
                                Leve_SelectionTab.Draw();
                            }

                            ImGui.TableNextColumn();
                            Theme_Colors.HeaderText($"Leve Details");
                            using (var settings_Child = ImRaii.Child("Leves Info Window", new Vector2(-1, -1), true))
                            {
                                Leve_DetailsTab.Draw();
                            }

                            ImGui.EndTable();
                        }

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Leve Manifest"))
                    {
                        Leve_WorklistTab.Draw();

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("ARR Gathering Grind"))
                    {
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Settings"))
                    {
                        ImGui.EndTabItem();
                    }

#if DEBUG
                    if (ImGui.BeginTabItem("Debug"))
                    {
                        Window_Debug.DrawDebugInfo();

                        ImGui.EndTabItem();
                    }
#endif

                    ImGui.EndTabBar();
                }
            }
        }
    }
}
