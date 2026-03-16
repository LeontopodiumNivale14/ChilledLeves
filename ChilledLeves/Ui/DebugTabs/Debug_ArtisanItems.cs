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
            // quick references for the sheets that I'm using
            var LeveSheet = Svc.Data.GetExcelSheet<Leve>();
            var RecipeSheet = Svc.Data.GetExcelSheet<Recipe>();

            if (ImGui.Button("Update Item List"))
            {
                AllItems.Clear();
                ArtisanPreCrafts.Clear();
                ArtisanFinalCrafts.Clear();

                foreach (var LeveEntry in C.workList)
                {
                    var LeveId = LeveEntry.LeveID;
                    if (!CraftDictionary.ContainsKey(LeveId))
                        continue;

                    var ItemId = CraftDictionary[LeveId].ItemID;
                    CheckItems(ItemId, LeveEntry.InputValue, RecipeSheet);

                    var RecipeRow = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == ItemId);

                    if (ArtisanFinalCrafts.ContainsKey(ItemId))
                        ArtisanFinalCrafts[ItemId] += LeveEntry.InputValue * RecipeRow.AmountResult.ToInt();
                    else
                        ArtisanFinalCrafts.Add(ItemId, (LeveEntry.InputValue * RecipeRow.AmountResult.ToInt()));
                }

                // Now break down all ingredients recursively
                foreach (var kvp in AllItems)
                {
                    var itemSearch = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == kvp.Key);
                    if (itemSearch.RowId != 0)
                    {
                        var itemId = kvp.Key;
                        var amount = kvp.Value;

                        if (!ArtisanPreCrafts.ContainsKey(itemId))
                            ArtisanPreCrafts.Add(itemId, amount);
                    }
                }
            }

            ImGui.Text("- - - All Items - - - ");
            foreach (var item in AllItems)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID [{item.Key}]: {itemName} | Amount: {item.Value}");
            }

            ImGui.Text("- - - Artisan Pre Crafts - - -");
            foreach (var item in ArtisanPreCrafts)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
            }

            ImGui.Spacing();

            ImGui.Text("- - - Artisan Final Crafts - - -");
            foreach (var item in ArtisanFinalCrafts)
            {
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

                ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
            }
        }

        private static void CheckItems(uint itemId, int runAmount, ExcelSheet<Recipe> RecipeSheet)
        {
            var recipe = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == itemId);
            if (recipe.RowId == 0)
                return; // Not a craftable item; likely a raw mat

            for (int i = 0; i < 6; i++)
            {
                PluginDebug($"Ingrediant[{i}]");

                // Checks to see if the ingrediant is valid
                var ingrediant = recipe.Ingredient[i].Value.RowId;
                if (ingrediant == 0)
                    continue;

                var amount = recipe.AmountIngredient[i].ToInt() * runAmount;

                if (AllItems.ContainsKey(ingrediant))
                {
                    PluginDebug($"Ingrediant: {ingrediant} exist currently, updating amount");
                    AllItems[ingrediant] += amount;
                }
                else if (!AllItems.ContainsKey(ingrediant))
                {
                    PluginDebug($"Ingrediant: {ingrediant} does not exist, adding to list");
                    AllItems.Add(ingrediant, amount);
                }

                var subRecipe = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == ingrediant);
                if (subRecipe.RowId != 0)
                {
                    // If the item is a recipe, check the recipe sheet for all ingredients
                    CheckItems(ingrediant, runAmount, RecipeSheet);
                }
            }

        }
    }
}
