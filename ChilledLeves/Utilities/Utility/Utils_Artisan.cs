using ChilledLeves.Utilities.LeveData;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public static void ExportSelectedListToTC()
    {
        string baseUrl = "https://ffxivteamcraft.com/import/";
        string exportItems = "";

        foreach (var leveEntry in C.LeveList)
        {
            var LeveId = leveEntry.Key;
            if (leveEntry.Value == 0)
                continue;

            if (LeveInfo.Leve_SheetInfo.TryGetValue(LeveId, out var sheetInfo))
            {
                var amount = sheetInfo.MaterialInfo.TurninAmount;
                var leveTurninCount = leveEntry.Value;
                var neededAmount = amount * leveTurninCount;
                if (C.AllowMultiTurnin)
                    neededAmount *= sheetInfo.MaterialInfo.RepeatAmount;

                var itemId = sheetInfo.MaterialInfo.Item_Id;
                exportItems += $"{itemId},null,{neededAmount};";
            }
        }

        exportItems = exportItems.TrimEnd(';');

        var plainTextBytes = Encoding.UTF8.GetBytes(exportItems);
        string base64 = Convert.ToBase64String(plainTextBytes);

        Svc.Log.Debug($"{baseUrl}{base64}");
        ImGui.SetClipboardText($"{baseUrl}{base64}");
        Notify.Success("Link copied to clipboard");
    }

    public static Dictionary<uint, int> AllItems = new();
    public static Dictionary<uint, int> ArtisanPreCrafts = new();
    public static Dictionary<uint, int> ArtisanFinalCrafts = new();

    public static void ExportPreCrafts()
    {
        var recipeSheet = ExcelHelper.Sheet_Recipe;

        foreach (var leveEntry in C.LeveList)
        {
            if (leveEntry.Value == 0)
                continue;

            var leveId = leveEntry.Key;

            if (LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var sheetInfo))
            {
                var itemId = sheetInfo.MaterialInfo.Item_Id;
                CheckItems(itemId, leveEntry.Value, recipeSheet);

                var recipe = recipeSheet.FirstOrDefault(r => r.ItemResult.RowId == itemId);

                var runAmount = leveEntry.Value * recipe.AmountResult.ToInt();

                if (ArtisanFinalCrafts.ContainsKey(itemId))
                    ArtisanFinalCrafts[itemId] += runAmount;
                else
                    ArtisanFinalCrafts.Add(itemId, runAmount);
            }
        }

        // Now break down all ingredients recursively
        foreach (var kvp in AllItems)
        {
            var itemSearch = recipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == kvp.Key);
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
