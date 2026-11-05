using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_CuratedList
    {
        #region State

        private static readonly HashSet<int> _selected = new();

        private static int _startId = 1001;
        private static bool _prefixFolderName = false;

        private static string _json = string.Empty;
        private static string _status = string.Empty;

        private static readonly JsonSerializerOptions ExportOptions = new()
        {
            WriteIndented = true,
        };

        #endregion

        #region Draw

        public static void Draw()
        {
            if (C.Leve_SavedPlaylist.Count == 0)
            {
                ImGui.TextUnformatted("No saved lists to export.");
                return;
            }

            DrawOptions();
            ImGui.Separator();
            DrawSelection();
            ImGui.Separator();
            DrawExportButtons();
            DrawOutput();
        }

        private static void DrawOptions()
        {
            ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
            ImGui.InputInt("Start ID", ref _startId, 0);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Exported lists get sequential IDs from here.\nKeep this the same between exports so IDs stay stable.");

            ImGui.Checkbox("Prefix list names with folder name", ref _prefixFolderName);

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("The curated panel has no folders, so this keeps the grouping visible.\nExample: \"Crafting - My List\"");

            if (ImGui.Button("Select all"))
            {
                foreach (var list in C.Leve_SavedPlaylist)
                    _selected.Add(list.Id);
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear"))
                _selected.Clear();

            ImGui.SameLine();
            ImGui.TextUnformatted($"{_selected.Count} selected");
        }

        private static void DrawSelection()
        {
            var height = Math.Min(300 * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y * 0.5f);

            using var child = ImRaii.Child("##CuratedExportSelection", new Vector2(-1, Math.Max(height, 100 * ImGuiHelpers.GlobalScale)), true);
            if (!child.Success)
                return;

            foreach (var folder in C.GetSortedFolders())
                DrawFolder(folder);

            foreach (var list in RootLists())
                DrawListCheckbox(list);
        }

        private static void DrawFolder(LeveInfo.SavedFolder folder)
        {
            using var id = ImRaii.PushId($"export_folder_{folder.Id}");

            var lists = ListsIn(folder.Id);
            var all = lists.Count > 0 && lists.All(l => _selected.Contains(l.Id));

            if (ImGui.Checkbox("##FolderSelect", ref all))
            {
                foreach (var list in lists)
                {
                    if (all)
                        _selected.Add(list.Id);
                    else
                        _selected.Remove(list.Id);
                }
            }

            ImGui.SameLine();
            using var node = ImRaii.TreeNode($"{folder.Name} ({lists.Count})###folder_{folder.Id}");
            if (!node.Success)
                return;

            foreach (var list in lists)
                DrawListCheckbox(list);
        }

        private static void DrawListCheckbox(LeveInfo.SavedList list)
        {
            using var id = ImRaii.PushId($"export_list_{list.Id}");

            var isSelected = _selected.Contains(list.Id);
            if (!ImGui.Checkbox($"{list.Name} ({list.Playlist.Count})", ref isSelected))
                return;

            if (isSelected)
                _selected.Add(list.Id);
            else
                _selected.Remove(list.Id);
        }

        private static void DrawExportButtons()
        {
            using (ImRaii.Disabled(_selected.Count == 0))
            {
                if (ImGui.Button("Copy JSON"))
                {
                    BuildJson();
                    ImGui.SetClipboardText(_json);
                    _status = $"Copied to the clipboard. {_status}";
                }

                ImGui.SameLine();
                if (ImGui.Button("Save to file"))
                    SaveToFile();
            }

            if (!string.IsNullOrEmpty(_status))
                ImGui.TextWrapped(_status);
        }

        private static void DrawOutput()
        {
            if (string.IsNullOrEmpty(_json))
                return;

            // Buffer needs room for the full string plus the null terminator
            var bufferSize = Math.Max(_json.Length + 1, 64);

            ImGui.InputTextMultiline("##CuratedExportPreview", ref _json, bufferSize,
                new Vector2(-1, -1), ImGuiInputTextFlags.ReadOnly);
        }

        #endregion

        #region Export

        private static void BuildJson()
        {
            var export = new List<LeveInfo.SavedList>();
            var skipped = 0;
            var nextId = _startId;

            foreach (var (folderName, list) in OrderedLists())
            {
                if (!_selected.Contains(list.Id))
                    continue;

                var playlist = new List<LeveInfo.Info_List>();
                foreach (var leve in list.Playlist)
                {
                    // Drop leves that don't exist in the sheet, they'd be dropped on load anyway
                    if (!LeveInfo.LeveExists(leve.Id))
                    {
                        skipped++;
                        continue;
                    }

                    playlist.Add(new() { Id = leve.Id, Amount = leve.Amount });
                }

                var name = _prefixFolderName && folderName != null
                    ? $"{folderName} - {list.Name}"
                    : list.Name;

                export.Add(new()
                {
                    Id = nextId++,
                    Name = name,
                    Description = list.Description ?? "",
                    Playlist = playlist,
                });
            }

            // Only the curated fields are written, FolderId/SortOrder/LastModified are left out
            var output = export.Select(l => new
            {
                l.Id,
                l.Name,
                l.Description,
                Playlist = l.Playlist.Select(p => new { p.Id, p.Amount }),
            });

            _json = JsonSerializer.Serialize(output, ExportOptions);
            _status = $"Exported {export.Count} list(s).";

            if (skipped > 0)
                _status += $" Skipped {skipped} unknown leve(s).";
        }

        private static void SaveToFile()
        {
            BuildJson();

            try
            {
                var path = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), "CuratedLevepacks.json");
                File.WriteAllText(path, _json);
                _status = $"Saved to {path}. {_status}";
            }
            catch (Exception ex)
            {
                Svc.Log.Error(ex, "Failed to save curated export");
                _status = $"Failed to save: {ex.Message}";
            }
        }

        #endregion

        #region Ordering

        private static List<LeveInfo.SavedList> ListsIn(int folderId) => C.Leve_SavedPlaylist
            .Where(l => l.FolderId == folderId)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // No folder, or the folder no longer exists
        private static List<LeveInfo.SavedList> RootLists() => C.Leve_SavedPlaylist
            .Where(l => l.FolderId == null || C.FindFolder(l.FolderId.Value) == null)
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Same order as the panel: folders first, then root lists
        private static IEnumerable<(string? FolderName, LeveInfo.SavedList List)> OrderedLists()
        {
            foreach (var folder in C.GetSortedFolders())
            {
                foreach (var list in ListsIn(folder.Id))
                    yield return (folder.Name, list);
            }

            foreach (var list in RootLists())
                yield return (null, list);
        }

        #endregion
    }
}
