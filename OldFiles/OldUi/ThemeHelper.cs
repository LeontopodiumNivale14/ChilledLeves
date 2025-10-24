using System.Numerics;
using System;
using System.Collections.Generic;

namespace ChilledLeves.Ui
{
    internal static class ThemeHelper
    {
        // Style tracking for debugging leaks
        private static int _globalStylePushCount = 0;
        private static int _globalStylePopCount = 0;
        
        // Color definitions for the ice theme - updated with user values
        public static readonly Vector4 IceBlue = new Vector4(0.7f, 0.85f, 1.0f, 1.0f);
        public static readonly Vector4 DarkIceBlue = new Vector4(0.3f, 0.5f, 0.7f, 1.0f);
        public static readonly Vector4 DeepIceBlue = new Vector4(0.15f, 0.25f, 0.4f, 1.0f);
        public static readonly Vector4 FrostWhite = new Vector4(0.95f, 0.97f, 1.0f, 1.0f);
        public static readonly Vector4 TranslucentIce = new Vector4(0.8f, 0.9f, 1.0f, 0.15f);
        public static readonly Vector4 DarkSlate = new Vector4(0.12f, 0.14f, 0.18f, 0.9f);
        
        // Style colors - derived from the color definitions
        public static readonly Vector4 ButtonBg = DarkIceBlue;
        public static readonly Vector4 ButtonHovered = new Vector4(DarkIceBlue.X + 0.1f, DarkIceBlue.Y + 0.1f, DarkIceBlue.Z + 0.1f, 1.0f);
        public static readonly Vector4 ButtonActive = new Vector4(DarkIceBlue.X + 0.15f, DarkIceBlue.Y + 0.15f, DarkIceBlue.Z + 0.15f, 1.0f);
        
        public static readonly Vector4 HeaderBg = new Vector4(0.23f, 0.35f, 0.48f, 0.55f);
        public static readonly Vector4 HeaderHovered = new Vector4(0.26f, 0.38f, 0.52f, 0.7f);
        public static readonly Vector4 HeaderActive = new Vector4(0.28f, 0.42f, 0.58f, 0.9f);
        
        public static readonly Vector4 FrameBg = new Vector4(0.18f, 0.22f, 0.28f, 1.0f);
        public static readonly Vector4 FrameBgHovered = new Vector4(0.23f, 0.27f, 0.33f, 1.0f);
        public static readonly Vector4 FrameBgActive = new Vector4(0.25f, 0.3f, 0.35f, 1.0f);
        
        public static readonly Vector4 ChildBg = new Vector4(0.15f, 0.18f, 0.22f, 0.7f);
        
        // Track global style pushing/popping
        // If these get out of sync, we know there's a leak
        private static int _totalStylesPushed = 0;
        private static int _totalStyleVarsPushed = 0;
        
        public static int BeginTheming(bool useIceTheme)
        {
            if (!useIceTheme)
                return 0;
            
            int styleCount = SafePushColor(ImGuiCol.WindowBg, new Vector4(0.15f, 0.17f, 0.2f, 0.94f));
            SafePushColor(ImGuiCol.Border, new Vector4(0.5f, 0.7f, 0.9f, 0.3f));
            SafePushColor(ImGuiCol.FrameBg, FrameBg);
            SafePushColor(ImGuiCol.FrameBgHovered, FrameBgHovered);
            SafePushColor(ImGuiCol.FrameBgActive, FrameBgActive);
            
            return 5; // Return the total number of styles pushed
        }
        
        public static void EndTheming(bool useIceTheme, int styleCount)
        {
            if (useIceTheme && styleCount > 0)
            {
                SafePopColor(styleCount);
            }
        }
        
        // Push a style color and track it globally
        public static int SafePushColor(ImGuiCol col, Vector4 color)
        {
            ImGui.PushStyleColor(col, color);
            _globalStylePushCount++;
            return 1;
        }
        
        // Safely pop style colors, with error checking
        public static void SafePopColor(int count)
        {
            if (count <= 0) return;
            
            try
            {
                ImGui.PopStyleColor(count);
                _globalStylePopCount += count;
            }
            catch
            {
                // In case we try to pop too many colors, log it
                Svc.Log.Error($"Attempted to pop {count} colors but failed");
            }
        }
        
        // Window styling (compatibility method)
        public static int PushWindowStyle()
        {
            return SafePushColor(ImGuiCol.WindowBg, DarkSlate);
        }
        
        // Button styling
        public static int PushButtonStyle()
        {
            int count = SafePushColor(ImGuiCol.Button, ButtonBg);
            SafePushColor(ImGuiCol.ButtonHovered, ButtonHovered);
            SafePushColor(ImGuiCol.ButtonActive, ButtonActive);
            return 3;
        }
        
        // Header/Tab styling
        public static int PushHeaderStyle()
        {
            int count = SafePushColor(ImGuiCol.Header, HeaderBg);
            SafePushColor(ImGuiCol.HeaderHovered, HeaderHovered);
            SafePushColor(ImGuiCol.HeaderActive, HeaderActive);
            return 3;
        }
        
        // Child window styling
        public static int PushChildStyle()
        {
            return SafePushColor(ImGuiCol.ChildBg, ChildBg);
        }
        
        // Heading text styling
        public static int PushHeadingTextStyle()
        {
            return SafePushColor(ImGuiCol.Text, IceBlue);
        }
        
        // Body text styling (added back for compatibility)
        public static int PushBodyTextStyle()
        {
            return SafePushColor(ImGuiCol.Text, FrostWhite);
        }
        
        // Control styling
        public static int PushControlStyle()
        {
            int count = SafePushColor(ImGuiCol.FrameBg, FrameBg);
            SafePushColor(ImGuiCol.FrameBgHovered, FrameBgHovered);
            SafePushColor(ImGuiCol.FrameBgActive, FrameBgActive);
            SafePushColor(ImGuiCol.CheckMark, IceBlue);
            return 4;
        }
        
        // Table styling
        public static int PushTableStyle()
        {
            int count = SafePushColor(ImGuiCol.TableHeaderBg, DeepIceBlue);
            SafePushColor(ImGuiCol.Header, HeaderBg);
            SafePushColor(ImGuiCol.HeaderHovered, HeaderHovered);
            SafePushColor(ImGuiCol.HeaderActive, HeaderActive);
            return 4;
        }
        
        // Get style leak stats (can be called to check for leaks)
        public static (int pushed, int popped) GetStyleStats()
        {
            return (_globalStylePushCount, _globalStylePopCount);
        }
        
        // Reset style tracking (useful for debugging)
        public static void ResetStyleTracking()
        {
            _globalStylePushCount = 0;
            _globalStylePopCount = 0;
        }
        
        // A utility class for scoped style pushing
        public class StyleScope : IDisposable
        {
            private int _colorCount = 0;
            private int _varCount = 0;
            private bool _disposed = false;
            
            public StyleScope() { }
            
            public StyleScope PushColor(ImGuiCol col, Vector4 color)
            {
                ImGui.PushStyleColor(col, color);
                _colorCount++;
                _globalStylePushCount++;
                return this;
            }
            
            public StyleScope PushVar(ImGuiStyleVar var, float value)
            {
                ImGui.PushStyleVar(var, value);
                _varCount++;
                return this;
            }
            
            public StyleScope PushVar(ImGuiStyleVar var, Vector2 value)
            {
                ImGui.PushStyleVar(var, value);
                _varCount++;
                return this;
            }
            
            public void Dispose()
            {
                if (_disposed) return;
                
                if (_colorCount > 0)
                {
                    ImGui.PopStyleColor(_colorCount);
                    _globalStylePopCount += _colorCount;
                }
                
                if (_varCount > 0)
                    ImGui.PopStyleVar(_varCount);
                
                _disposed = true;
            }
        }
    }
} 