using ChilledLeves.Scheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui
{
    internal class Window_Overlay : Window
    {
        public Window_Overlay() : base($"Chilled Leves Overlay ##ChilledLevesOverlayv1")
        {
            Flags = ImGuiWindowFlags.None;
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        public override bool DrawConditions()
        {
#if DEBUG
            return true;
#endif
            return !Leve_Helper.IsIdle && C.ShowActiveOverlay;
        }

        public override void Draw()
        {
            ImGui.Text($"Current Status: {Leve_Helper.State}");
        }
    }
}
