using ChilledLeves.Utilities;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Debug_ArtisanItems
    {
        public static void CraftingDebug()
        {
            var allItems = Utils.AllItems;
            var artisanPreCrafts = Utils.ArtisanPreCrafts;
            var artisanFinalCrafts = Utils.ArtisanFinalCrafts;

            if (ImGui.Button("Update Item List"))
            {
                allItems.Clear();
                artisanPreCrafts.Clear();
                artisanFinalCrafts.Clear();

                Utils.ExportPreCrafts();
            }

            ImGui.Text("- - - All Items - - - ");
            foreach (var item in allItems)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID [{item.Key}]: {itemName} | Amount: {item.Value}");
            }

            ImGui.Text("- - - Artisan Pre Crafts - - -");
            foreach (var item in artisanPreCrafts)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
            }

            ImGui.Spacing();

            ImGui.Text("- - - Artisan Final Crafts - - -");
            foreach (var item in artisanFinalCrafts)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
            }
        }
    }
}
