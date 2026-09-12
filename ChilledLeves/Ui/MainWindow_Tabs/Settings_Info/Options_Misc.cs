using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;

public static partial class SettingsUi
{
    private static readonly string MiscOption = "Misc Options";
    private static SettingEntry ColorTheme = new()
    {
        Label = "Use Custom Theme",
        Category = MiscOption,
        Keywords = new[] {"Theme", "Color", "Ice Theme"},
        Draw = () =>
        {
            var v = C.UseIceTheme;
            if (ImGui.Checkbox("Use Ice Theme", ref v))
            {
                C.UseIceTheme = v;
                C.Save();
            }
        }
    };

    private static SettingEntry ShowActiveOverlay = new()
    {
        Label = "Show Active Overlay",
        Category = MiscOption,
        Keywords = new[] {"Overlay", "Active Overlay", "Progress"},
        Draw = () =>
        {
            var v = C.ShowActiveOverlay;
            if (ImGui.Checkbox("Show progress overlay while running", ref v))
            {
                C.ShowActiveOverlay = v;
                C.Save();
            }
        }
    };
}
