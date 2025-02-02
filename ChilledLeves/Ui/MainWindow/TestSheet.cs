using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.MainWindow;

internal class TestSheet
{
    internal static void Draw()
    {
        if (ImRaii.Table("Test Table", 2, ImGuiTableFlags.Reorderable))
        {
            ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("Starting City", ImGuiTableColumnFlags.WidthFixed, 200);

            ImGui.TableHeadersRow();

            foreach (var kdp in LeveDict)
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{kdp.Value.LeveName}");

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{kdp.Value.ZoneName}");
            }

            ImGui.EndTable();
        }
    }
}
