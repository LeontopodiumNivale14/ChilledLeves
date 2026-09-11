using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Ui.DebugTabs;
using ChilledLeves.Ui.MainWindow_Tabs;
using ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;
using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System.Collections.Generic;
using System.Reflection;

namespace ChilledLeves.Ui
{
    internal class Window_Main : Window
    {
        public Window_Main() : base($"Chilled Leves [{P.GetType().Assembly.GetName().Version}] ### ChilledLevesMainWindowV2")
        {
            Namespace = "ChilledLevesV2";
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
                colors.Push(ImGuiCol.Button, Theme_Colors.ButtonBg);
                colors.Push(ImGuiCol.ButtonHovered, Theme_Colors.ButtonHovered);
                colors.Push(ImGuiCol.ButtonActive, Theme_Colors.ButtonActive);
                colors.Push(ImGuiCol.WindowBg, Theme_Colors.DarkSlate);
                colors.Push(ImGuiCol.FrameBg, Theme_Colors.FrameBg);
                colors.Push(ImGuiCol.FrameBgHovered, Theme_Colors.FrameBgHovered);
                colors.Push(ImGuiCol.FrameBgActive, Theme_Colors.FrameBgActive);
                colors.Push(ImGuiCol.CheckMark, Theme_Colors.IceBlue);
                colors.Push(ImGuiCol.Text, Theme_Colors.FrostWhite);
            }

            using var style = C.UseIceTheme ? ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 4.0f) : default;

            SelectableSidebar();
            ImGui.SameLine();
            DetailsInfo();
        }

        public class WindowInfo
        {
            public FontAwesomeIcon Icon { get; set; }
            public string Label { get; set; }
            public Action Draw { get; set; }
        }

        public Dictionary<WindowSelection, WindowInfo> TabItems = new()
        {
            [WindowSelection.LeveInfo] = new()
            {
                Icon = FontAwesomeIcon.BookBookmark,
                Label = "Leve Info",
                Draw = () => Leve_Details.Draw()
            },
            [WindowSelection.LeveManifest] = new()
            {
                Icon = FontAwesomeIcon.PersonChalkboard,
                Label = "Leve Playlist",
                Draw = () => Leve_WorklistTab.Draw(),
            },
            [WindowSelection.ARR_Grind] = new()
            {
                Icon = FontAwesomeIcon.PersonThroughWindow,
                Label = "ARR Grind",
                Draw = () => { }
            },
            [WindowSelection.UserInfo] = new()
            {
                Icon = FontAwesomeIcon.Users,
                Label = "User Info",
                Draw = () => User_Info.Draw()
            },
            [WindowSelection.Settings] = new()
            {
                Icon = FontAwesomeIcon.Cogs,
                Label = "Settings",
                Draw = () => SettingsUi.Draw()
            },
            [WindowSelection.GatherProfiles] = new()
            {
                Icon = FontAwesomeIcon.Leaf,
                Label = "Gather Profiles",
                Draw = () => Gathering_Profiles.Draw()
            },
            [WindowSelection.Logs] = new()
            {
                Icon = FontAwesomeIcon.Clipboard,
                Label = "Logs",
                Draw = () => User_Logs.Draw(),
            },
            [WindowSelection.Debug] = new()
            {
                Icon = FontAwesomeIcon.Qrcode,
                Label = "Debug",
                Draw = () => { }
            },
        };

        private void SelectableSidebar()
        {
            var scale = ImGuiHelpers.GlobalScale;
            int baseSize = 200;
            var scaledWidth = baseSize * scale;
            var height = ImGui.GetContentRegionAvail().Y;

            // TODO: I want to make this adjustable based on the size of the window...
            // var compact = ImGui.GetContentRegionAvail().X < (500 * scale); 
            var compact = false;

            using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

            using (var sidebarSelection = ImRaii.Child("CL_SidebarSelection", new(scaledWidth, height), true))
            {
                if (!sidebarSelection.Success)
                    return;

                PluginIcon();
                float avail = ImGui.GetContentRegionAvail().X;

                using var table = ImRaii.Table("##tabList", 3, ImGuiTableFlags.None);
                if (table)
                {
                    ImGui.TableSetupColumn("accent", ImGuiTableColumnFlags.WidthFixed, 4 * scale);
                    ImGui.TableSetupColumn("icon", ImGuiTableColumnFlags.WidthFixed, 24 * scale);
                    ImGui.TableSetupColumn("label", compact ? ImGuiTableColumnFlags.WidthFixed : ImGuiTableColumnFlags.WidthStretch, compact ? 0f : 0f);

                    foreach (var item in TabItems)
                    {
                        DrawSelectable_Icon(item.Value.Icon, item.Value.Label, item.Key, compact);
                    }
                }
            }
        }
        private void DetailsInfo()
        {
            var selectedTab = C.SelectedTab;
            if (TabItems.TryGetValue(selectedTab, out var tabInfo))
            {
                ImGui.SameLine();
                tabInfo.Draw();
            }
            else
            {
                ImGui.Text($"Can't find the tab: {selectedTab}");
            }
        }
        private static void PluginIcon()
        {
            const string iconPath = "ChilledLeves.images.icon.png";

            var pluginIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), iconPath).GetWrapOrEmpty();
            if (pluginIcon == null) return;

            // Getting the image size via the texture wrap (using the * after to manipulate the size cause, she big)
            var imageSize = new Vector2(pluginIcon.Width, pluginIcon.Height) * 0.35f;

            // Calculate the offset/centering here
            var sidebarWidth = ImGui.GetContentRegionAvail().X;
            var offsetX = (sidebarWidth - imageSize.X) / 2.0f;

            // Center and drawing the image now
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offsetX);
            ImGui.Image(pluginIcon.Handle, imageSize);

            ImGui.Dummy(new(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new(0, 5));

            var currentAmount = Utils.Allowances / 100;
            ImGui_Ice.Draw_XPBar(Utils.Allowances, 100, $"Allowances: {Utils.Allowances}/100");
            ImGui.Text($"Next in: {Utils.NextAllowances:hh':'mm':'ss}");

            // Add spacing after image
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }
        public static bool RowSelectable(string id, bool isSelected, float height = 0f)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));

            bool clicked = ImGui.Selectable(id, isSelected,
                ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, height));

            ImGui.PopStyleColor(3);

            if (isSelected)
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderActive));
            else if (ImGui.IsItemHovered())
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderHovered));

            return clicked;
        }
        public static void DrawSelectable_Icon(FontAwesomeIcon icon, string label, WindowSelection tab, bool compact)
        {
            bool isSelected = C.SelectedTab == tab;
            float scale = ImGuiHelpers.GlobalScale;
            float rowHeight = 25 * scale;

            ImGui.TableNextRow();

            // --- Column 0: accent bar (paints just this cell when selected) ---
            ImGui.TableNextColumn();
            if (isSelected)
            {
                var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;

                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
            }

            // --- Column 1: icon ---
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Dummy(new(0.1f));
            ImGui.SameLine();
            ImGui_Ice.Icon(icon);

            // --- Column 2: label + row-spanning selectable ---
            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();

            string selectableLabel = compact ? $"##{tab}" : $"{label}##{tab}";
            bool clicked = RowSelectable(selectableLabel, isSelected, rowHeight);

            if (clicked)
            {
                C.SelectedTab = tab;
                C.SaveDebounced();
            }

            if (compact && ImGui.IsItemHovered())
                ImGui.SetTooltip(label);
        }
    }
}
