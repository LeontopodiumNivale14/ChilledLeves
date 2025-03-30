using ImGuiNET;
using System.Numerics;

namespace ChilledLeves.Ui
{
    public static class ThemeHelper
    {

        public static Vector4 IceBlue = new Vector4(0.7f, 0.85f, 1.0f, 1.0f);
        public static Vector4 DarkIceBlue = new Vector4(0.3f, 0.5f, 0.7f, 1.0f);
        public static Vector4 DeepIceBlue = new Vector4(0.15f, 0.25f, 0.4f, 1.0f);
        public static Vector4 FrostWhite = new Vector4(0.95f, 0.97f, 1.0f, 1.0f);
        public static Vector4 TranslucentIce = new Vector4(0.8f, 0.9f, 1.0f, 0.15f);
        public static Vector4 DarkSlate = new Vector4(0.12f, 0.14f, 0.18f, 0.9f);
        
        public static Vector4 ButtonBg => DarkIceBlue;
        public static Vector4 ButtonHovered => new Vector4(DarkIceBlue.X + 0.1f, DarkIceBlue.Y + 0.1f, DarkIceBlue.Z + 0.1f, 1.0f);
        public static Vector4 HeaderBg => new Vector4(0.23f, 0.35f, 0.48f, 0.55f);
        public static Vector4 HeaderHovered => new Vector4(0.26f, 0.38f, 0.52f, 0.7f);
        public static Vector4 HeaderActive => new Vector4(0.28f, 0.42f, 0.58f, 0.9f);
        public static Vector4 FrameBg => new Vector4(0.18f, 0.22f, 0.28f, 1.0f);
        public static Vector4 FrameBgHovered => new Vector4(0.23f, 0.27f, 0.33f, 1.0f);
        public static Vector4 FrameBgActive => new Vector4(0.25f, 0.3f, 0.35f, 1.0f);
        public static Vector4 ChildBg => new Vector4(0.15f, 0.18f, 0.22f, 0.7f);

        private static int _totalStylesPushed = 0;
        
        /// <param name="usingIceTheme">Whether to apply the ice theme</param>
        public static void BeginTheming(bool usingIceTheme)
        {
            _totalStylesPushed = 0;
            
            if (usingIceTheme)
            {

                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    ImGui.PushStyleColor((ImGuiCol)i, ImGui.GetStyle().Colors[i]);
                }
                _totalStylesPushed = (int)ImGuiCol.COUNT;
                

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, ImGui.GetStyle().WindowPadding);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, ImGui.GetStyle().FramePadding);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, ImGui.GetStyle().ItemSpacing);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, ImGui.GetStyle().ItemInnerSpacing);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, ImGui.GetStyle().FrameRounding);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, ImGui.GetStyle().WindowRounding);
                
                ImGui.PushStyleColor(ImGuiCol.WindowBg, DarkSlate);
                _totalStylesPushed++;
            }
        }
        

        /// <param name="usingIceTheme">Whether the ice theme is being used</param>
        public static void EndTheming(bool usingIceTheme)
        {
            if (usingIceTheme)
            {
                if (_totalStylesPushed > 0)
                {
                    ImGui.PopStyleColor(_totalStylesPushed);
                    _totalStylesPushed = 0;
                }
                
                ImGui.PopStyleVar(6); 
            }
        }
        
        public static int PushWindowStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, DarkSlate);
            return 1;
        }
        
        public static int PushButtonStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ButtonBg);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ButtonHovered);
            return 2;
        }
        
        public static int PushHeaderStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Header, HeaderBg);
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, HeaderHovered);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, HeaderActive);
            return 3;
        }

        public static int PushControlStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, FrameBg);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, FrameBgHovered);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, FrameBgActive);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, IceBlue);
            return 4;
        }
        
        public static int PushChildStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, ChildBg);
            return 1;
        }
        
        public static int PushHeadingTextStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, IceBlue);
            return 1;
        }
        
        public static int PushBodyTextStyle()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, FrostWhite);
            return 1;
        }
    }
} 