using ChilledLeves.Utilities.LeveData;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    #region Saved Data

    public List<LeveInfo.SavedList> Leve_SavedPlaylist { get; set; } = new();
    public List<LeveInfo.SavedFolder> Leve_SavedFolders { get; set; } = new();
    public LeveInfo.ListSortMode SortMode { get; set; } = LeveInfo.ListSortMode.Manual;

    #endregion

    #region Lists

    public LeveInfo.SavedList? FindPlaylist(int id) => Leve_SavedPlaylist.Find(c => c.Id == id);

    public void SaveNewPlaylist(string Name, string description)
    {
        List<LeveInfo.Info_List> playlist = new();
        foreach (var leve in LeveOrder)
            playlist.Add(new() { Id = leve, Amount = LeveList[leve] });

        Leve_SavedPlaylist.Add(new()
        {
            Id = GenerateListId(),
            Name = Name,
            Description = description,
            LastModified = DateTime.UtcNow,
            SortOrder = NextListSortOrder(null),
            Playlist = playlist
        });
        C.Save();
    }

    public void RenameList(LeveInfo.SavedList list, string name)
    {
        list.Name = name;
        list.Touch();
    }

    #endregion

    #region Folders

    public LeveInfo.SavedFolder? FindFolder(int id) => Leve_SavedFolders.Find(f => f.Id == id);

    public List<LeveInfo.SavedFolder> GetSortedFolders() => Leve_SavedFolders
        .OrderBy(f => f.SortOrder)
        .ThenBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    public void AddFolder(string name = "New Folder")
    {
        Leve_SavedFolders.Add(new()
        {
            Id = NextFolderId(),
            Name = name,
            SortOrder = NextFolderSortOrder()
        });
    }

    public void DeleteFolder(int folderId)
    {
        // Lists fall back to the root instead of being deleted
        foreach (var list in Leve_SavedPlaylist.Where(l => l.FolderId == folderId))
            list.FolderId = null;

        Leve_SavedFolders.RemoveAll(f => f.Id == folderId);
    }

    #endregion

    #region Ordering

    // Lists dropped into a folder go to the bottom of it
    public void MoveListToFolder(LeveInfo.SavedList list, int? folderId)
    {
        if (list.FolderId == folderId)
            return;

        list.SortOrder = NextListSortOrder(folderId);
        list.FolderId = folderId;
    }

    // direction: -1 = up, +1 = down
    public void MoveList(LeveInfo.SavedList list, int direction)
    {
        var siblings = Leve_SavedPlaylist
            .Where(l => l.FolderId == list.FolderId)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        MoveWithin(siblings, list, direction, (l, order) => l.SortOrder = order);
    }

    public void MoveFolder(LeveInfo.SavedFolder folder, int direction)
    {
        var ordered = GetSortedFolders();
        MoveWithin(ordered, folder, direction, (f, order) => f.SortOrder = order);
    }

    // Puts `dragged` right before/after `target`, in target's folder
    public void MoveListNextTo(LeveInfo.SavedList dragged, LeveInfo.SavedList target, bool after)
    {
        if (dragged.Id == target.Id)
            return;

        // Treat a dangling FolderId as root
        var folderId = target.FolderId != null && FindFolder(target.FolderId.Value) != null
            ? target.FolderId
            : null;

        var siblings = Leve_SavedPlaylist
            .Where(l => l.FolderId == folderId && l.Id != dragged.Id)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var index = siblings.FindIndex(l => l.Id == target.Id);
        if (index < 0)
            index = siblings.Count;
        else if (after)
            index++;

        dragged.FolderId = folderId;
        siblings.Insert(index, dragged);

        for (var i = 0; i < siblings.Count; i++)
            siblings[i].SortOrder = i;
    }

    public void MoveFolderNextTo(LeveInfo.SavedFolder dragged, LeveInfo.SavedFolder target, bool after)
    {
        if (dragged.Id == target.Id)
            return;

        var ordered = GetSortedFolders();
        ordered.RemoveAll(f => f.Id == dragged.Id);

        var index = ordered.FindIndex(f => f.Id == target.Id);
        if (index < 0)
            index = ordered.Count;
        else if (after)
            index++;

        ordered.Insert(index, dragged);

        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SortOrder = i;
    }

    private static void MoveWithin<T>(List<T> ordered, T item, int direction, Action<T, int> setOrder)
    {
        var index = ordered.IndexOf(item);
        if (index < 0)
            return;

        var target = index + direction;
        if (target < 0 || target >= ordered.Count)
            return;

        (ordered[index], ordered[target]) = (ordered[target], ordered[index]);

        // Renumber 0..n-1 so lists that all started at SortOrder 0 get a clean order
        for (var i = 0; i < ordered.Count; i++)
            setOrder(ordered[i], i);
    }

    #endregion

    #region Id / Order Helpers

    private int GenerateListId()
    {
        var rng = new Random();
        var id = rng.Next(1, 50000);
        while (FindPlaylist(id) != null)
            id = rng.Next(1, 50000);

        return id;
    }

    private int NextFolderId() =>
        Leve_SavedFolders.Count == 0 ? 1 : Leve_SavedFolders.Max(f => f.Id) + 1;

    private int NextFolderSortOrder() =>
        Leve_SavedFolders.Count == 0 ? 0 : Leve_SavedFolders.Max(f => f.SortOrder) + 1;

    // Bottom of a folder, or of the root when folderId is null
    private int NextListSortOrder(int? folderId)
    {
        var siblings = Leve_SavedPlaylist.Where(l => l.FolderId == folderId).ToList();
        return siblings.Count == 0 ? 0 : siblings.Max(l => l.SortOrder) + 1;
    }

    #endregion

    #region Import / Export

    private const string SharePrefix = "CL1:";
    private const int MaxImportBytes = 5 * 1024 * 1024;

    private static readonly JsonSerializerOptions ShareOptions = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public class ShareData
    {
        public int Version { get; set; } = 1;
        public List<LeveInfo.SavedFolder> Folders { get; set; } = new();
        public List<LeveInfo.SavedList> Lists { get; set; } = new();
    }

    public record ImportResult(bool Success, string Message);

    public string ExportAll() => Export(Leve_SavedFolders, Leve_SavedPlaylist);

    // Takes subsets too, so a single list or folder can be exported
    public string Export(IEnumerable<LeveInfo.SavedFolder> folders, IEnumerable<LeveInfo.SavedList> lists)
    {
        var data = new ShareData { Folders = folders.ToList(), Lists = lists.ToList() };
        var json = JsonSerializer.SerializeToUtf8Bytes(data, ShareOptions);

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            gzip.Write(json, 0, json.Length);

        return SharePrefix + Convert.ToBase64String(output.ToArray());
    }

    public ImportResult Import(string? text)
    {
        var data = Decode(text);
        if (data == null)
            return new(false, "The clipboard doesn't contain valid ChilledLeves data.");

        if (data.Folders.Count == 0 && data.Lists.Count == 0)
            return new(false, "There was nothing to import.");

        var folderMap = ImportFolders(data);
        var (importedLists, skippedLeves) = ImportLists(data, folderMap);

        var message = $"Imported {importedLists} list(s) and {data.Folders.Count} folder(s).";
        if (skippedLeves > 0)
            message += $" Skipped {skippedLeves} unknown leve(s).";

        return new(true, message);
    }

    // Returns exporter's folder id -> new folder id on this machine
    private Dictionary<int, int> ImportFolders(ShareData data)
    {
        var folderMap = new Dictionary<int, int>();

        foreach (var source in data.Folders.OrderBy(f => f.SortOrder))
        {
            var folder = new LeveInfo.SavedFolder
            {
                Id = NextFolderId(),
                Name = string.IsNullOrWhiteSpace(source.Name) ? "Imported Folder" : source.Name,
                SortOrder = NextFolderSortOrder(),
            };

            Leve_SavedFolders.Add(folder);
            folderMap[source.Id] = folder.Id;
        }

        return folderMap;
    }

    private (int Imported, int SkippedLeves) ImportLists(ShareData data, Dictionary<int, int> folderMap)
    {
        var imported = 0;
        var skipped = 0;

        foreach (var source in data.Lists.OrderBy(l => l.SortOrder))
        {
            // Lists whose folder wasn't part of the export land in the root
            int? folderId = null;
            if (source.FolderId is { } oldFolderId && folderMap.TryGetValue(oldFolderId, out var newFolderId))
                folderId = newFolderId;

            var leves = new List<LeveInfo.Info_List>();
            foreach (var leve in source.Playlist ?? new())
            {
                // Drops leves that don't exist here (bad data, or removed in a patch)
                if (leve == null || !LeveInfo.LeveExists(leve.Id))
                {
                    skipped++;
                    continue;
                }

                leves.Add(new()
                {
                    Id = leve.Id,
                    Amount = Math.Max(0, leve.Amount)
                });
            }

            Leve_SavedPlaylist.Add(new()
            {
                Id = GenerateListId(),
                Name = string.IsNullOrWhiteSpace(source.Name) ? "Imported List" : source.Name,
                Description = source.Description ?? "",
                FolderId = folderId,
                SortOrder = NextListSortOrder(folderId),
                LastModified = DateTime.UtcNow,
                Playlist = leves
            });

            imported++;
        }

        return (imported, skipped);
    }

    private static ShareData? Decode(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        text = text.Trim();
        if (!text.StartsWith(SharePrefix, StringComparison.Ordinal))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(text[SharePrefix.Length..]);
            using var input = new MemoryStream(bytes);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);

            // Cap the decompressed size so a hostile string can't balloon in memory
            using var decompressed = new MemoryStream();
            var buffer = new byte[8192];
            int read;
            while ((read = gzip.Read(buffer, 0, buffer.Length)) > 0)
            {
                decompressed.Write(buffer, 0, read);
                if (decompressed.Length > MaxImportBytes)
                    return null;
            }

            decompressed.Position = 0;
            var data = JsonSerializer.Deserialize<ShareData>(decompressed, ShareOptions);
            if (data == null)
                return null;

            data.Folders = (data.Folders ?? new()).Where(f => f != null).ToList();
            data.Lists = (data.Lists ?? new()).Where(l => l != null).ToList();
            return data;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}