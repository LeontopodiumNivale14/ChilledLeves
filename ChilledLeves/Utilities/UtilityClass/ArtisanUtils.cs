using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities.UtilityClass
{
    public static partial class Utils
    {
        /*
        public static void ExportSelectedListToTC()
        {
            string baseUrl = "https://ffxivteamcraft.com/import/";
            string exportItems = "";

            foreach (var leveEntry in C.workList)
            {
                var LeveId = leveEntry.LeveID;

                if (!CraftDictionary.ContainsKey(LeveId))
                    continue;

                var itemAmount = CraftDictionary[LeveId].TurninAmount;

                var InputAmount = leveEntry.InputValue;
                var AmountNeeded = InputAmount * itemAmount;

                var ItemId = CraftDictionary[LeveId].ItemID;

                exportItems += $"{ItemId},null,{AmountNeeded};";
            }

            exportItems = exportItems.TrimEnd(';');

            var plainTextBytes = Encoding.UTF8.GetBytes(exportItems);
            string base64 = Convert.ToBase64String(plainTextBytes);

            Svc.Log.Debug($"{baseUrl}{base64}");
            ImGui.SetClipboardText($"{baseUrl}{base64}");
            Notify.Success("Link copied to clipboard");
        }
        */

        public static Dictionary<uint, int> AllItems = new();
        public static Dictionary<uint, int> ArtisanPreCrafts = new();
        public static Dictionary<uint, int> ArtisanFinalCrafts = new();

        /*
        public static void ExportPreCrafts()
        {
            var LeveSheet = Svc.Data.GetExcelSheet<Leve>();
            var RecipeSheet = Svc.Data.GetExcelSheet<Recipe>();

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

            string preCrafts = "Pre crafts :\n";

            foreach (var kvp in ArtisanPreCrafts)
            {
                var itemId = kvp.Key;
                var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(itemId).Name.ToString();
                var amount = kvp.Value;

                string temp = $"{amount}x {itemName}\n";
                preCrafts += temp;
            }

            ImGui.SetClipboardText(preCrafts);
        }
        */

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
