using ChilledLeves.Ui.MainWindow_Tabs.Leve_Info;
using ChilledLeves.Ui.Old_Ui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_Details
    {
        public static void Draw()
        {
            var globalScale = ImGuiHelpers.GlobalScale;
            float settingsWidth = 250 * globalScale;
            float leveSelection = 500 * globalScale;

            using (var leve_ChildContainer = ImRaii.Child("Details: Main", new(settingsWidth, -1)))
            {
                if (!leve_ChildContainer.Success)
                    return;

                Theme_Colors.HeaderText("Main Settings");

                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;
                using (var leve_SelectionSettings = ImRaii.Child("Setting Child Window", new Vector2(-1, -1), true))
                {
                    if (!leve_SelectionSettings.Success)
                        return;

                    Leve_MainTab.Draw();
                }
            }
            ImGui.SameLine();

            using (var leve_selectionContainer = ImRaii.Child("Details: Selection", new Vector2(leveSelection, -1)))
            {
                if (!leve_selectionContainer.Success)
                    return;

                Theme_Colors.HeaderText($"Showing {Leve_SelectionTab.LeveCount} / {Leve_SelectionTab.Leve_Total}");

                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;
                using (var settings_Child = ImRaii.Child("Leve Details Window", new Vector2(-1, -1), true))
                {
                    Leve_SelectionTab.Draw();
                }
            }

            ImGui.SameLine();

            using (var leve_DetailsContainer = ImRaii.Child("Details: LeveInfo", new(-1, -1)))
            {
                if (!leve_DetailsContainer.Success)
                    return;

                Theme_Colors.HeaderText($"Leve Details");
                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;
                using (var settings_Child = ImRaii.Child("Leves Info Window", new Vector2(-1, -1), true))
                {
                    Leve_DetailsTab.Draw();
                }
            }
        }
    }
}
