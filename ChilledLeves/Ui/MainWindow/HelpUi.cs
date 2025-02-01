using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.MainWindow;

internal static unsafe class HelpUi
{
    internal static void Draw()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();

        if (sheet != null)
        {
            if (ImRaii.Table("Test Table", 4))
            {
                ImGui.TableSetupColumn("Leve #");
                ImGui.TableSetupColumn("Job Type");
                ImGui.TableSetupColumn("Leve Name");
                ImGui.TableSetupColumn("Item Name");

                ImGui.TableHeadersRow();

                foreach (var row in sheet)
                {
                    // Tells me what row # we're on. Not generally useful for a lot of sheets but. Can atleast let me check for things in the future.
                    // In the case of leves, the leveID is tied to the row number... for now. Till square decides to take this away ;-;
                    uint leveNumber = row.RowId;

                    // Checking to see if the starting city is even valid. This will filter out the blank ones from the sheet and let me not get garbo
                    uint town = sheet.GetRow(leveNumber).Town.RowId;
                    if (town != 0)
                    {
                        // Dummy test to make sure that the leves are even popping up properly
                        // ImGui.Text($"Leve # {levenumber}");

                        // Check to see what the job type is
                        uint leveJob = sheet.GetRow(leveNumber).LeveAssignmentType.Value.RowId;

                        // Name of the leve that you're grabbing
                        string leveName = sheet.GetRow(leveNumber).Name.ToString();

                        // The questID of the leve. Need this for another sheet but also, might be useful to check progress of other quest...
                        var questID = sheet.GetRow(leveNumber).DataId.RowId;

                        // Item that is required for the leve to be turned in
                        // Uses the questID to look into another sheet to find which item you need to have for turnin
                        // THIS IS CURRENTLY BROKEN THOUGH. NOT SURE WHY
                        uint itemID = Svc.Data.GetExcelSheet<CraftLeve>().GetRow(questID).Item[0].RowId;
                        string itemName = Svc.Data.GetExcelSheet<Item>().GetRow(itemID).Name.ToString();

                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{leveNumber}");

                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text($"{leveJob}");

                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text(leveName);

                        ImGui.TableSetColumnIndex(3);
                        ImGui.Text($"{itemName}");

                    }
                }
            }

            ImGui.EndTable();

            /*
            string leveName = Svc.Data.GetExcelSheet<Leve>().GetRow(i).Name.ToString();
            if (Svc.Data.GetExcelSheet<Leve>().GetRow(i).Town.Value.RowId != 0)
            ImGui.Text($"{leveName}");
            */
        }
    }
}
