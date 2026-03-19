using ChilledLeves.Resources;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_GatherEditor
    {
        private static FileDialogManager fileDialogManager = new FileDialogManager();

        private static uint LeveSelected = 0;

        private static void Draw()
        {
            var AllRoutes = RouteLoader.Leve_Routes;

            if (ImGui.Button("Create All Routes"))
            {

            }

            if (ImGui.BeginTable("ChilledLeves: Route Editors", 2))
            {

            }
        }
    }
}
