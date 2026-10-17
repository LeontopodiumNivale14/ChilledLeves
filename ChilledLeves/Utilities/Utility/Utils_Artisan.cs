using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;
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
                var leveTurninCount = leveEntry.Value;
                var neededAmount = Leve_RequiredAmount(sheetInfo.MaterialInfo) * leveTurninCount;

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

    private const int MaxIngredientSlots = 8;   // Slots 0-5 are normal ingredients, 6-7 are crystals/shards
    private const int NormalIngredientSlots = 6;

    public static Dictionary<uint, int> ArtisanPreCrafts = new();
    public static Dictionary<uint, int> ArtisanFinalCrafts = new();
    public static Dictionary<uint, int> RawMaterials = new();

    /// <summary>
    /// True if the last Calculate subtracted inventory. The UI and exports read this
    /// so they always match what was actually calculated.
    /// </summary>
    public static bool UsedInventory { get; private set; }

    // Inventory counts captured during Calculate, so every number stays consistent until the next Calculate.
    private static readonly Dictionary<uint, int> OwnedSnapshot = new();

    // Built once. TryAdd keeps the first recipe per item, same as the old FirstOrDefault.
    private static readonly Lazy<Dictionary<uint, Recipe>> RecipeByResult = new(() =>
    {
        var map = new Dictionary<uint, Recipe>();
        foreach (var recipe in ExcelHelper.Sheet_Recipe)
        {
            if (recipe.ItemResult.RowId == 0)
                continue;

            map.TryAdd(recipe.ItemResult.RowId, recipe);
        }
        return map;
    });

    public static void ExportFinalCrafts(bool includeCrystals = false)
        => ExportToClipboard("Final crafts", ArtisanFinalCrafts);

    public static void ExportPreCrafts(bool includeCrystals = false)
        => ExportToClipboard("Pre crafts", ArtisanPreCrafts);

    public static void ExportRawMaterials(bool includeCrystals = false)
        => ExportToClipboard("Raw materials", RawMaterials);

    // Crystal item IDs: shards 2-7, crystals 8-13, clusters 14-19.
    private const uint FirstCrystalId = 2;
    private const uint LastCrystalId = 19;

    private static bool IsCrystal(uint itemId)
    {
        return itemId >= FirstCrystalId && itemId <= LastCrystalId;
    }

    /// <summary>
    /// Sends the amounts still needed of the current RawMaterials to GatherChill.
    /// Returns how many items GatherChill added or updated.
    /// </summary>
    public static int ExportToGather(bool includeCrystals)
    {
        if (RawMaterials.Count == 0)
            return 0;

        if (!P.gatherChill.IsReady())
            return 0;

        var items = new List<(uint ItemId, int Amount)>();
        foreach (var (itemId, amount) in RawMaterials)
        {
            if (!includeCrystals && IsCrystal(itemId))
                continue;

            var needed = GetNeeded(itemId, amount);
            if (needed <= 0)
                continue;

            items.Add((itemId, needed));
        }

        if (items.Count == 0)
            return 0;

        return P.gatherChill.TryAddToGatherList(items.ToArray());
    }

    private static void ExportToClipboard(string title, Dictionary<uint, int> items)
    {
        var sb = new StringBuilder();
        AppendSection(sb, title, items);
        ImGui.SetClipboardText(sb.ToString());
    }

    public static void Calculate(bool includeCrystals, bool useInventory)
    {
        ArtisanPreCrafts.Clear();
        ArtisanFinalCrafts.Clear();
        RawMaterials.Clear();
        OwnedSnapshot.Clear();
        UsedInventory = useInventory;

        var recipes = RecipeByResult.Value;

        foreach (var (leveId, runs) in C.LeveList.Where(x => C.LeveOrder.Contains(x.Key)))
        {
            if (runs <= 0) // skip entries with zero (or negative) runs
                continue;

            if (!LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var sheetInfo))
                continue;

            var itemId = sheetInfo.MaterialInfo.Item_Id;
            if (!recipes.TryGetValue(itemId, out var recipe))
                continue;

            var amountNeeded = runs * Leve_RequiredAmount(sheetInfo.MaterialInfo);
            AddCrafts(ArtisanFinalCrafts, itemId, recipe, amountNeeded, includeCrystals);
        }
    }

    /// <summary>
    /// How many of this item the player owned when Calculate ran (NQ + HQ).
    /// Always 0 when inventory wasn't used.
    /// </summary>
    public static int GetOwned(uint itemId)
    {
        if (!UsedInventory)
            return 0;

        if (OwnedSnapshot.TryGetValue(itemId, out var count))
            return count;

        count = GetItemCount(itemId);
        OwnedSnapshot[itemId] = count;
        return count;
    }

    /// <summary>
    /// How many more of this item are still needed after what the player owns.
    /// Never negative.
    /// </summary>
    public static int GetNeeded(uint itemId, int amount)
        => Math.Max(0, amount - GetOwned(itemId));

    /// <summary>
    /// How many times the recipe for this item must be crafted to cover what is still needed.
    /// Returns 0 if the item has no recipe (raw material) or nothing more is needed.
    /// </summary>
    public static int GetCraftCount(uint itemId, int amount)
    {
        var needed = GetNeeded(itemId, amount);
        if (needed <= 0)
            return 0;

        if (!RecipeByResult.Value.TryGetValue(itemId, out var recipe))
            return 0;

        return CraftsNeeded(recipe, needed);
    }

    private static int CraftsNeeded(Recipe recipe, int amount)
    {
        var yield = Math.Max((int)recipe.AmountResult, 1);
        return (amount + yield - 1) / yield;
    }

    /// <summary>
    /// Adds to the target dictionary (total demand), then expands ingredients only for the
    /// crafts that the newly *needed* amount (demand minus owned) requires.
    /// </summary>
    private static void AddCrafts(Dictionary<uint, int> target, uint itemId, Recipe recipe, int amountNeeded, bool includeCrystals)
    {
        if (amountNeeded <= 0)
            return;

        var before = target.GetValueOrDefault(itemId);
        var after = before + amountNeeded;
        target[itemId] = after;

        var netBefore = GetNeeded(itemId, before);
        var netAfter = GetNeeded(itemId, after);

        var extraCrafts = CraftsNeeded(recipe, netAfter) - CraftsNeeded(recipe, netBefore);
        if (extraCrafts <= 0) // no new crafts needed: owned items or leftovers from a previous craft cover it
            return;

        AddIngredients(recipe, extraCrafts, includeCrystals);
    }

    private static void AddIngredients(Recipe recipe, int crafts, bool includeCrystals)
    {
        var slotCount = includeCrystals ? MaxIngredientSlots : NormalIngredientSlots;

        for (var i = 0; i < slotCount; i++)
        {
            var ingredientId = recipe.Ingredient[i].RowId;
            if (ingredientId == 0)
                continue;

            Expand(ingredientId, recipe.AmountIngredient[i] * crafts, includeCrystals);
        }
    }

    private static void Expand(uint itemId, int amountNeeded, bool includeCrystals)
    {
        if (amountNeeded <= 0) // nothing needed, stop
            return;

        if (!RecipeByResult.Value.TryGetValue(itemId, out var recipe))
        {
            AddAmount(RawMaterials, itemId, amountNeeded);
            return;
        }

        AddCrafts(ArtisanPreCrafts, itemId, recipe, amountNeeded, includeCrystals);
    }

    private static void AddAmount(Dictionary<uint, int> dict, uint itemId, int amount)
        => dict[itemId] = dict.GetValueOrDefault(itemId) + amount;

    private static void AppendSection(StringBuilder sb, string title, Dictionary<uint, int> items)
    {
        var itemSheet = Svc.Data.GetExcelSheet<Item>();

        sb.AppendLine($"{title} :");
        foreach (var (itemId, amount) in items)
        {
            var needed = GetNeeded(itemId, amount);
            if (needed <= 0)
                continue;

            var name = itemSheet.TryGetRow(itemId, out var item) ? item.Name.ToString() : $"Unknown ({itemId})";
            sb.AppendLine($"{needed}x {name}");
        }
    }
}