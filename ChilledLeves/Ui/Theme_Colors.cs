using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal static class Theme_Colors
    {
        // ---- Base palette ----
        public static readonly Vector4 IceBlue = new Vector4(0.7f, 0.85f, 1.0f, 1.0f);
        public static readonly Vector4 DarkIceBlue = new Vector4(0.3f, 0.5f, 0.7f, 1.0f);
        public static readonly Vector4 DeepIceBlue = new Vector4(0.15f, 0.25f, 0.4f, 1.0f);
        public static readonly Vector4 FrostWhite = new Vector4(0.95f, 0.97f, 1.0f, 1.0f);
        public static readonly Vector4 TranslucentIce = new Vector4(0.8f, 0.9f, 1.0f, 0.15f);
        public static readonly Vector4 DarkSlate = new Vector4(0.12f, 0.14f, 0.18f, 0.9f);
        public static readonly Vector4 LightSlate = new Vector4(0.22f, 0.26f, 0.33f, 0.9f);

        // Helpers to make the colors fit in with the rest of the base colors from above ^

        private static Vector4 WithAlpha(Vector4 c, float alpha)
            => new Vector4(c.X, c.Y, c.Z, alpha);

        // Relative lighten (percentage of remaining headroom to white) —
        // scales sanely whether the base is dark or already fairly light
        private static Vector4 LightenRel(Vector4 c, float t)
            => new Vector4(
                c.X + (1.0f - c.X) * t,
                c.Y + (1.0f - c.Y) * t,
                c.Z + (1.0f - c.Z) * t,
                c.W);

        // Standard interaction ramp: same t-values everywhere, so any
        // base color hovers/actives with the same *relative* punch
        private const float HoverT = 0.15f;
        private const float ActiveT = 0.28f;

        private const float HeaderHoverT = 0.08f;
        private const float HeaderActiveT = 0.15f;

        // Sidebar Accent (specifically for the table cell)
        public static readonly Vector4 SidebarAccent = IceBlue;

        // Buttons
        public static readonly Vector4 ButtonBg = DarkIceBlue;
        public static readonly Vector4 ButtonHovered = LightenRel(DarkIceBlue, HoverT);
        public static readonly Vector4 ButtonActive = LightenRel(DarkIceBlue, ActiveT);

        // Header
        public static readonly Vector4 HeaderBg = WithAlpha(DarkIceBlue, 0.55f);
        public static readonly Vector4 HeaderHovered = WithAlpha(LightenRel(DarkIceBlue, HeaderHoverT), 0.6f);
        public static readonly Vector4 HeaderActive = WithAlpha(LightenRel(DarkIceBlue, HeaderActiveT), 0.65f);

        // Frame
        public static readonly Vector4 FrameBg = WithAlpha(LightSlate, 1.0f);
        public static readonly Vector4 FrameBgHovered = LightenRel(FrameBg, HoverT * 0.6f);
        public static readonly Vector4 FrameBgActive = LightenRel(FrameBg, ActiveT * 0.6f);

        // Child
        public static readonly Vector4 ChildBg = WithAlpha(DarkSlate, 0.7f);

        public static void CustomHeader(string text, float width, float height = 30f)
        {
            var useCustomColor = C.UseIceTheme;

            // Save current cursor position
            var cursorPos = ImGui.GetCursorPos();

            // Draw the header background
            var drawList = ImGui.GetWindowDrawList();
            var screenPos = ImGui.GetCursorScreenPos();
            var headerColor = useCustomColor ? ImGui.GetColorU32(DeepIceBlue) : ImGui.GetColorU32(ImGuiCol.Header);

            drawList.AddRectFilled(screenPos, new Vector2(screenPos.X + width, screenPos.Y + height), headerColor);

            // Calculate text centering
            var textSize = ImGui.CalcTextSize(text);
            var textPosX = screenPos.X + (width - textSize.X) / 2f;
            var textPosY = screenPos.Y + (height - textSize.Y) / 2f;

            // Draw centered text
            var textColor = useCustomColor ? ImGui.GetColorU32(FrostWhite) : ImGui.GetColorU32(ImGuiCol.Text);
            drawList.AddText(new Vector2(textPosX, textPosY), textColor, text);

            // Move cursor past the header
            ImGui.SetCursorPos(new Vector2(cursorPos.X, cursorPos.Y + height));
            ImGui.Dummy(new Vector2(width, 0)); // Reserve horizontal space
        }

        public static void HeaderText(string text)
        {
            bool themeUsage = C.UseIceTheme;
            var color = C.UseIceTheme ? IceBlue : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            ImGuiEx.Text(color, text);
        }
    }
}
