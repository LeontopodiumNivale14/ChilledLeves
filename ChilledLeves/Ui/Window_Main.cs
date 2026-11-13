using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Ui.DebugTabs;
using ChilledLeves.Ui.MainWindow_Tabs;
using ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;
using ChilledLeves.Utilities;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;
using System.Reflection;

namespace ChilledLeves.Ui
{
    internal class Window_Main : Window
    {
        public Window_Main() : base($"Chilled Leves [{P.GetType().Assembly.GetName().Version}] ###ChilledLeves_MainWindow")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new(200, 200)
            };
            P.windowSystem.AddWindow(this);
            TitleBarButtons.Add(new() { ShowTooltip = () => ImGui.SetTooltip("♥ Ko-fi (Buy me an ice coffee)"), Icon = FontAwesomeIcon.Heart, IconOffset = new(1, 1), Click = _ => GenericHelpers.ShellStart("https://ko-fi.com/ice643269") });
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
            public string HoverHint { get; set; } = string.Empty;
        }

        public Dictionary<WindowSelection, WindowInfo> TabItems = new()
        {
            [WindowSelection.LeveInfo] = new()
            {
                Icon = FontAwesomeIcon.BookBookmark,
                Label = "Leve Info",
                Draw = () => Leve_Details.Draw(),
                HoverHint = "See what all leves you have / what you need to complete"
            },
            [WindowSelection.LeveManifest] = new()
            {
                Icon = FontAwesomeIcon.PersonChalkboard,
                Label = "Leve Playlist",
                Draw = () => Leve_Playlist.Draw(),
                HoverHint = "Formally named worklist\n" +
                            "Run all the leves you have queue'd up in a row"
            },
            [WindowSelection.SavedList] = new()
            {
                Icon = FontAwesomeIcon.Save,
                Label = "Saved Playlist",
                Draw = () => Leve_List.Draw(),
                HoverHint = "View all your saved playlist, and also curated ones as well"
            },
            [WindowSelection.ARR_Grind] = new()
            {
                Icon = FontAwesomeIcon.PersonThroughWindow,
                Label = "Gathering Grind",
                Draw = () => Leve_ARRGrind.Draw(),
                HoverHint = "Also known as ARR/HW Grind\n" +
                            "Used to grind gatherer's up since you need to reroll\n" +
                            "everything from ARR->HW for BTN/MIN/FSH"
            },
            [WindowSelection.UserInfo] = new()
            {
                Icon = FontAwesomeIcon.Users,
                Label = "User Info",
                Draw = () => User_Info.Draw(),
                HoverHint = "Keep track of your characters and how many leves they have"
            },
            [WindowSelection.Settings] = new()
            {
                Icon = FontAwesomeIcon.Cogs,
                Label = "Settings",
                Draw = () => SettingsUi.Draw(),
                HoverHint = "Plugin settings... nothing else here"
            },
            [WindowSelection.GatherProfiles] = new()
            {
                Icon = FontAwesomeIcon.Leaf,
                Label = "Gather Profiles",
                Draw = () => Gathering_Profiles.Draw(),
                HoverHint = "Set what gathering buffs you want to use on BTN/MIN mission kinds"
            },
            [WindowSelection.NpcInfo] = new()
            {
                Icon = FontAwesomeIcon.PersonWalkingLuggage,
                Label = "Levemetes",
                Draw = () => Levemetes_Info.Draw(),
                HoverHint = "See what levemete has what leve",
            },
            [WindowSelection.Logs] = new()
            {
                Icon = FontAwesomeIcon.Clipboard,
                Label = "Logs",
                Draw = () => User_Logs.Draw(),
                HoverHint = "You know how I ask you to see logs if you have a bug report?\n" +
                            "Yeah, it's this tab"
            },
            [WindowSelection.RouteEditor] = new()
            {
                Icon = FontAwesomeIcon.Route,
                Label = "Route Editor",
                Draw = () => Route_Editor.Draw(),
            },
            [WindowSelection.Debug] = new()
            {
                Icon = FontAwesomeIcon.Qrcode,
                Label = "Debug",
                Draw = () => Ui_LeveInfo.Draw()
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
                        if (C.Compact_Ui)
                            DrawSelectable_Icon_Small(item.Value, item.Key, compact);
                        else
                            DrawSelectable_Icon(item.Value, item.Key, compact);
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
        public static void DrawSelectable_Icon(WindowInfo info, WindowSelection selection, bool compact)
        {
            FontAwesomeIcon icon = info.Icon;
            string label = info.Label;
            WindowSelection tab = selection;


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
            if (ImGui.IsItemHovered() && info.HoverHint != string.Empty)
            {
                ImGui.SetTooltip(info.HoverHint);
            }

            if (clicked)
            {
                C.SelectedTab = tab;
                C.SaveDebounced();
            }

            if (compact && ImGui.IsItemHovered())
                ImGui.SetTooltip(label);
        }
        public static void DrawSelectable_Icon_Small(WindowInfo info, WindowSelection selection, bool compact)
        {
            FontAwesomeIcon icon = info.Icon;
            string label = info.Label;
            WindowSelection tab = selection;

            bool SmallRowSelectable(string id, bool isSelected, float height = 0f)
            {
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));

                // Center the label vertically within the given height
                ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0f, 0.5f));

                bool clicked = ImGui.Selectable(id, isSelected,
                    ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, height));

                ImGui.PopStyleVar();
                ImGui.PopStyleColor(3);

                if (isSelected)
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderActive));
                else if (ImGui.IsItemHovered())
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderHovered));

                return clicked;
            }

            bool isSelected = C.SelectedTab == tab;
            float scale = ImGuiHelpers.GlobalScale;

            float textHeight = ImGui.GetTextLineHeight();
            float rowHeight = textHeight + 4 * scale;

            // Measure the icon with the icon font so centering is accurate
            ImGui.PushFont(UiBuilder.IconFont);
            float iconHeight = ImGui.GetTextLineHeight();
            ImGui.PopFont();
            float iconOffsetY = (rowHeight - iconHeight) * 0.5f;

            ImGui.TableNextRow(ImGuiTableRowFlags.None, rowHeight);

            // --- Column 0: accent bar (paints just this cell when selected) ---
            ImGui.TableNextColumn();
            if (isSelected)
            {
                var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;
                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
            }

            // --- Column 1: icon, centered within the row ---
            ImGui.TableNextColumn();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + iconOffsetY);
            ImGui.Dummy(new(0.1f));
            ImGui.SameLine();
            ImGui_Ice.Icon(icon);

            // --- Column 2: label + row-spanning selectable (label is centered) ---
            ImGui.TableNextColumn();

            string selectableLabel = compact ? $"##{tab}" : $"{label}##{tab}";
            bool clicked = SmallRowSelectable(selectableLabel, isSelected, rowHeight);

            if (ImGui.IsItemHovered() && info.HoverHint != string.Empty)
                ImGui.SetTooltip(info.HoverHint);

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
