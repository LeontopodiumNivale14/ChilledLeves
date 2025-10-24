using Dalamud.Interface.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities.UtilityClass
{
    public static partial class Utils
    {
        public static void FancyCheckmark(bool enabled)
        {
            float columnWidth = ImGui.GetColumnWidth();  // Get column width
            float rowHeight = ImGui.GetTextLineHeightWithSpacing();  // Get row height

            Vector2 iconSize = ImGui.CalcTextSize($"{FontAwesome.Cross}"); // Get icon size
            float iconWidth = iconSize.X;
            float iconHeight = iconSize.Y;

            float cursorX = ImGui.GetCursorPosX() + (columnWidth - iconWidth) * 0.5f;
            float cursorY = ImGui.GetCursorPosY() + (rowHeight - iconHeight) * 0.5f;

            cursorX = Math.Max(cursorX, ImGui.GetCursorPosX()); // Prevent negative padding
            cursorY = Math.Max(cursorY, ImGui.GetCursorPosY());

            ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

            if (!enabled)
            {
                FontAwesome.Print(ImGuiColors.DalamudRed, FontAwesome.Cross);
            }
            else if (enabled)
            {
                FontAwesome.Print(ImGuiColors.HealerGreen, FontAwesome.Check);
            }
        }
        public static void FavoriteImage()
        {
            var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
            Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
            Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);
            ImGui.Image(starTex.Handle, new Vector2(18, 18), uvMin, uvMax);
        }
    }
}
