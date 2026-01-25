using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal static class Theme_Colors
    {
        // Color definitions for the ice theme - updated with user values
        public static readonly Vector4 IceBlue = new Vector4(0.7f, 0.85f, 1.0f, 1.0f);
        public static readonly Vector4 DarkIceBlue = new Vector4(0.3f, 0.5f, 0.7f, 1.0f);
        public static readonly Vector4 DeepIceBlue = new Vector4(0.15f, 0.25f, 0.4f, 1.0f);
        public static readonly Vector4 FrostWhite = new Vector4(0.95f, 0.97f, 1.0f, 1.0f);
        public static readonly Vector4 TranslucentIce = new Vector4(0.8f, 0.9f, 1.0f, 0.15f);
        public static readonly Vector4 DarkSlate = new Vector4(0.12f, 0.14f, 0.18f, 0.9f);
        public static readonly Vector4 LightSlate = new Vector4(0.22f, 0.26f, 0.33f, 0.9f);

        // Style colors - derived from the color definitions
        public static readonly Vector4 ButtonHovered = new Vector4(DarkIceBlue.X + 0.1f, DarkIceBlue.Y + 0.1f, DarkIceBlue.Z + 0.1f, 1.0f);
        public static readonly Vector4 ButtonActive = new Vector4(DarkIceBlue.X + 0.15f, DarkIceBlue.Y + 0.15f, DarkIceBlue.Z + 0.15f, 1.0f);

        public static readonly Vector4 HeaderBg = new Vector4(0.23f, 0.35f, 0.48f, 0.55f);
        public static readonly Vector4 HeaderHovered = new Vector4(0.26f, 0.38f, 0.52f, 0.7f);
        public static readonly Vector4 HeaderActive = new Vector4(0.28f, 0.42f, 0.58f, 0.9f);

        public static readonly Vector4 FrameBg = new Vector4(0.18f, 0.22f, 0.28f, 1.0f);
        public static readonly Vector4 FrameBgHovered = new Vector4(0.23f, 0.27f, 0.33f, 1.0f);
        public static readonly Vector4 FrameBgActive = new Vector4(0.25f, 0.3f, 0.35f, 1.0f);

        public static readonly Vector4 ChildBg = new Vector4(0.15f, 0.18f, 0.22f, 0.7f);

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

        public static void BodyText(string text)
        {
            using var textColor = ImRaii.PushColor(ImGuiCol.Text, FrostWhite);
            var color = C.UseIceTheme ? FrostWhite : ImGui.GetStyle().Colors[(int)ImGuiCol.Text];
            ImGuiEx.Text(color, text);
        }

        public static void HeaderSpacing()
        {
            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 5));
        }
    }
}
