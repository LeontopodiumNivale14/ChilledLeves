using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.MainWindow;

internal class TestSheet
{
    // This mainly exist so I can test the sheets and look at the other two w/ a reference.

    private static bool isEnabled = true;
    private static bool testBEnabled = false;

    internal static void Draw()
    {

        if (isEnabled)
        {
            if (ImRaii.Table("Test Table", 4, ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Key/RowID", ImGuiTableColumnFlags.WidthFixed, 75);
                ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Starting City", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("ItemID", ImGuiTableColumnFlags.WidthFixed, 100);

                ImGui.TableHeadersRow();

                foreach (var kdp in LeveDict)
                {
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{kdp.Key}");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{kdp.Value.LeveName}");

                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text($"{kdp.Value.ZoneName}");

                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text($"{kdp.Value.ItemID}");
                }

                ImGui.EndTable();
            }
        }

        if (testBEnabled)
        {
            var sheet2 = Svc.Data.GetExcelSheet<CraftLeve>();
            uint row = LeveDict[21].QuestID;
            uint itemID = sheet2.GetRow(row).Item[0].Value.RowId;

            ImGui.Text($"itemID = {itemID}");
        }


    }
}
