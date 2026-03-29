using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui;

public static class ImGui_Ice
{
    public static void IconWithTooltip(FontAwesomeIcon icon, string? tooltip = null, bool sameLine = true)
    {
        if (sameLine)
            ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        if (tooltip != null)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(tooltip);
            }
        }
    }
}
