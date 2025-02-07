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
    private static float Column2Width = 0;
    private static float Column3Width = 0;
    private static float Column4Width = 0;

    internal static void Draw()
    {
        if (ImGui.InputInt("LeveKind", ref JobSelected))
        {
            if (JobSelected < 4)
                JobSelected = 4;
            else if (JobSelected > 12)
                JobSelected = 12;
        }

        if (isEnabled)
        {
            if (ImRaii.Table("Test Table", 7, ImGuiTableFlags.Reorderable | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("Key/RowID", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, Column2Width);
                ImGui.TableSetupColumn("Starting City", ImGuiTableColumnFlags.WidthFixed, Column3Width);
                ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthFixed, Column4Width);
                ImGui.TableSetupColumn("Amount Needed", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Amount Have", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Class");

                ImGui.TableHeadersRow();

                foreach (var kdp in LeveDict)
                {


                    if (JobSelected != kdp.Value.JobID && JobSelected != 4)
                        continue;

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{kdp.Key}");

                    ImGui.TableSetColumnIndex(1);
                    uint JobId = kdp.Value.JobID;
                    if (LeveTypeDict[JobId].AssignmentIcon != null)
                    {
                        ImGui.Image(LeveTypeDict[JobId].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
                        ImGui.SameLine(25);
                    }
                    string LeveName = kdp.Value.LeveName;

                    ImGui.Text($"{LeveName}");
                    float LeveTextWidth = ImGui.CalcTextSize(LeveName).X + 30.0f; // Add padding
                    Column2Width = Math.Max(Column2Width, LeveTextWidth);

                    ImGui.TableSetColumnIndex(2);
                    string StartingCity = kdp.Value.StartingZoneName;

                    ImGui.Text($"{StartingCity}");
                    float CityTextWidth = ImGui.CalcTextSize(StartingCity).X + 15.0f; // Add padding
                    Column3Width = Math.Max(Column3Width, CityTextWidth);

                    ImGui.TableSetColumnIndex(3);
                    if (kdp.Value.ItemIcon != null)
                    {
                        ImGui.Image(kdp.Value.ItemIcon.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
                        ImGui.SameLine(25);
                    }

                    string ItemName = kdp.Value.ItemName;
                    ImGui.Text($"{ItemName}");

                    float ItemTextWidth = ImGui.CalcTextSize(ItemName).X + 30.0f; // Add padding
                    Column3Width = Math.Max(Column3Width, ItemTextWidth);

                    ImGui.TableSetColumnIndex(4);
                    int turninAmount = kdp.Value.TurninAmount;
                    ImGui.Text(turninAmount.ToString());

                    ImGui.TableSetColumnIndex(5);
                    ImGui.Text($"{kdp.Value.CurrentItemAmount}");

                    ImGui.TableSetColumnIndex(6);
                    ImGui.Text($"{LeveTypeDict[JobId].LeveClassType}");

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
