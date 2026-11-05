using System.Collections.Generic;
using System.Text.Json;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public enum ListSortMode
    {
        Name,
        RecentlyModified,
        Manual
    }

    public class SavedFolder
    {
        public int Id { get; set; }
        public string Name { get; set; } = "New Folder";
        public int SortOrder { get; set; } = 0;
    }

    public class SavedList
    {
        public int Id { get; set; }
        public string Name { get; set; } = "New";
        public string Description { get; set; } = "";
        public int? FolderId { get; set; } = null;
        public int SortOrder { get; set; } = 0;
        public DateTime LastModified { get; set; } = DateTime.MinValue;
        public List<Info_List> Playlist { get; set; } = new();

        public void Touch() => LastModified = DateTime.UtcNow;
    }

    public class Info_List
    {
        public uint Id { get; set; }
        public int Amount { get; set; }
    }

    public static readonly List<SavedList> Curated_Levepacks = new();

    public static SavedList? FindList(int id) => Curated_Levepacks.Find(c => c.Id == id);

    #region Leve Lookups

    private static readonly Dictionary<uint, string> _leveNameCache = new();

    #region Leve Lookups

    public static bool LeveExists(uint leveId) => Leve_SheetInfo.ContainsKey(leveId);

    public static string GetLeveName(uint leveId) =>
        Leve_SheetInfo.TryGetValue(leveId, out var info) ? info.LeveName.ToString() : string.Empty;

    #endregion

    #region Curated Loading

    private static readonly JsonSerializerOptions CuratedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    // Call once at startup, after Leve_SheetInfo is populated, so unknown leves can be dropped
    public static void LoadCuratedLevepacks()
    {
        Curated_Levepacks.Clear();

        var assembly = typeof(LeveInfo).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("CuratedLevepacks.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
            return;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return;

        List<SavedList>? lists;
        try
        {
            lists = JsonSerializer.Deserialize<List<SavedList>>(stream, CuratedOptions);
        }
        catch (Exception ex)
        {
            // Swap for your logger of choice
            ECommons.DalamudServices.Svc.Log.Error(ex, "Failed to load CuratedLevepacks.json");
            return;
        }

        if (lists == null)
            return;

        foreach (var list in lists)
        {
            if (list == null)
                continue;

            // Drop leves that no longer exist (removed in a patch)
            list.Playlist = (list.Playlist ?? new())
                .Where(l => l != null && LeveExists(l.Id))
                .ToList();

            list.Description ??= "";
            Curated_Levepacks.Add(list);
        }
    }

    #endregion

    #endregion
}