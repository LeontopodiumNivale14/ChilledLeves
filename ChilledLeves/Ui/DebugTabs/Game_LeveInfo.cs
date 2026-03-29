using ChilledLeves.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_LeveInfo
    {
        public static void Draw()
        {
            var activeLeves = Utils.Leve_ActiveIds();

            ImGui.Text($"Active Leves: {activeLeves.Count()}");
            if (activeLeves.Count() != 0)
            {
                if (ImGui.BeginTable("Leve Sequence Tracker", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    foreach (var leve in activeLeves)
                    {
                        ImGui.TableSetupColumn("ID");
                        ImGui.TableSetupColumn("Sequence");
                        ImGui.TableHeadersRow();

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{leve}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{Utils.Leve_Sequence(leve)}");
                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
