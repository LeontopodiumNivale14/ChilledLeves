using ChilledLeves.Ui.DebugUi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui
{
    internal class DebugWindow : Window
    {
        public DebugWindow() : base($"ChilledLeves | Debug Window {P.GetType().Assembly.GetName().Version}")
        {
            Flags = ImGuiWindowFlags.None;
            SizeConstraints = new()
            {
                MinimumSize = new Vector2(1050, 650)
            };
            P.windowSystem.AddWindow(this);
        }

        public void Dispose()
        {
            P.windowSystem.RemoveWindow(this);
        }

        private static int selectedDebugIndex = 0;
        private static string[] DebugTypes = 
        [
            "Leve Viewer",
            "Icon Viewer",
        ];

        public override void Draw()
        {
            Vector2 availableSize = ImGui.GetContentRegionAvail();
            float childHeight = availableSize.Y - 10;

            if (ImGui.BeginTable("Debug Window Selector", 2, ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Tab Selector", ImGuiTableColumnFlags.WidthFixed, 200.0f);
                ImGui.TableSetupColumn("Debug Info Window", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                if (ImGui.BeginChild("Debug Selector", new Vector2(0, childHeight), true))
                {
                    for (int i = 0; i < DebugTypes.Length; i++)
                    {
                        bool isSelected = (selectedDebugIndex == i);
                        string label = isSelected ? $"→ {DebugTypes[i]}" : $"   {DebugTypes[i]}";

                        if (ImGui.Selectable(label, isSelected))
                        {
                            selectedDebugIndex = i;
                        }
                    }
                    ImGui.EndChild();
                }

                ImGui.TableSetColumnIndex(1);

                if (ImGui.BeginChild("Debug Window Info", new Vector2(0, childHeight), true))
                {
                    switch (selectedDebugIndex)
                    {
                        case 1: Ui_IconViewer.Draw(); break;
                        default: ImGui.Text("Debugger Window"); break;
                    }
                    ImGui.EndChild();
                }

                ImGui.EndTable();
            }
        }
    }
}
