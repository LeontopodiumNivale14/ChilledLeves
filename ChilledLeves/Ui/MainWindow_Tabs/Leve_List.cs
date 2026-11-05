using ChilledLeves.Gui;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_List
    {
        #region Tabs

        public static void Draw()
        {
            using (var child = ImRaii.Child("Leve Tab Container", default))
            {
                if (!child.Success)
                    return;

                using (var tabBar = ImRaii.TabBar("Leve List: Tab Bar"))
                {
                    if (!tabBar.Success)
                        return;

                    using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

                    PersonalList();
                    LevePacks();
                }
            }
        }

        private static void PersonalList()
        {
            using (var personalTab = ImRaii.TabItem("Personal List"))
            {
                if (!personalTab.Success)
                    return;

                using (var child = ImRaii.Child("Personal List Tab Details", default))
                {
                    if (!child.Success)
                        return;

                    LeveSelectionWindow(_userPanel, C.Leve_SavedPlaylist);
                    ImGui.SameLine();
                    LevePlaylistDetails(_userPanel, C.Leve_SavedPlaylist);
                }
            }
        }

        private static void LevePacks()
        {
            using (var levepackTab = ImRaii.TabItem("Currated List"))
            {
                if (!levepackTab.Success)
                    return;

                using (var child = ImRaii.Child("Curated List Tab Details", default))
                {
                    if (!child.Success)
                        return;

                    LeveSelectionWindow(_curatedPanel, LeveInfo.Curated_Levepacks);
                    ImGui.SameLine();
                    LevePlaylistDetails(_curatedPanel, LeveInfo.Curated_Levepacks);
                }
            }
        }

        #endregion

        #region Leve Saved List

        #region State

        // Both panels (user + curated) share all the drawing code. The only difference
        // is that the curated one is read-only and has its own search box and selection.
        private sealed class ListPanelState
        {
            public readonly string Id;
            public readonly bool ReadOnly;
            public string Search = "";
            public int SelectedId = 0;

            public ListPanelState(string id, bool readOnly)
            {
                Id = id;
                ReadOnly = readOnly;
            }
        }

        private static readonly ListPanelState _userPanel = new("User", false);
        private static readonly ListPanelState _curatedPanel = new("Curated", true);

        private static string _renameBuffer = "";
        private static int _renameFolderId = -1;
        private static int _renameListId = -1;
        private static bool _openRenamePopup = false;

        private const string ListPayload = "CL_LIST";
        private const string FolderPayload = "CL_FOLDER";
        private static int _draggedListId = -1;
        private static int _draggedFolderId = -1;

        private const ImGuiDragDropFlags LineFlags = ImGuiDragDropFlags.AcceptBeforeDelivery | ImGuiDragDropFlags.AcceptNoDrawDefaultRect;

        private static bool HasSearch(ListPanelState panel) => !string.IsNullOrWhiteSpace(panel.Search);

        // Reordering only makes sense on the user panel, in Manual mode, and not while search flattens everything
        private static bool CanReorder(ListPanelState panel) =>
            !panel.ReadOnly && C.SortMode == LeveInfo.ListSortMode.Manual && !HasSearch(panel);

        #endregion

        #region Main Draw

        private static void LeveSelectionWindow(ListPanelState panel, List<LeveInfo.SavedList> leveList)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var style = ImGui.GetStyle();

            var width = 200 * scale;
            var padding = style.WindowPadding.Y * 2;

            // User panel: toolbar is two rows (search + folder button, then sort) and there's a footer.
            // Curated panel: just a search row, no footer.
            var toolbarRows = panel.ReadOnly ? 1 : 2;
            var topHeight = ImGui.GetFrameHeight() * toolbarRows + style.ItemSpacing.Y * (toolbarRows - 1) + padding;
            var bottomHeight = panel.ReadOnly ? 0 : ImGui.GetFrameHeight() + padding;
            var gaps = panel.ReadOnly ? 1 : 2;

            var middleHeight = Math.Max(
                ImGui.GetContentRegionAvail().Y - topHeight - bottomHeight - style.ItemSpacing.Y * gaps,
                50 * scale);

            // Open + draw at the parent level so the popup IDs match
            if (!panel.ReadOnly)
            {
                if (_openRenamePopup)
                {
                    ImGui.OpenPopup("RenameEntry");
                    _openRenamePopup = false;
                }
                DrawRenamePopup();
            }

            // Group makes the children behave as one block for SameLine()
            using var group = ImRaii.Group();

            using (var top = ImRaii.Child($"##LeveToolbar_{panel.Id}", new Vector2(width, topHeight), true))
            {
                if (top.Success)
                    DrawListToolbar(panel);
            }

            using (var middle = ImRaii.Child($"##LeveListScroll_{panel.Id}", new Vector2(width, middleHeight), true))
            {
                if (middle.Success)
                    DrawListContents(panel, leveList);
            }

            if (panel.ReadOnly)
                return;

            using (var bottom = ImRaii.Child($"##LeveFooter_{panel.Id}", new Vector2(width, bottomHeight), true))
            {
                if (bottom.Success)
                    DrawImportExportFooter(panel);
            }
        }

        private static void DrawListContents(ListPanelState panel, List<LeveInfo.SavedList> leveList)
        {
            // Read-only or searching: flat, sorted results, folders ignored
            if (panel.ReadOnly || HasSearch(panel))
            {
                foreach (var list in SortLists(panel, leveList.Where(l => ListMatchesSearch(panel, l))))
                    DrawListEntry(panel, list);

                return;
            }

            // Folders first
            foreach (var folder in SortFolders())
                DrawFolder(panel, folder, leveList);

            // Then root lists (no folder, or folder no longer exists)
            var rootLists = leveList
                .Where(l => l.FolderId == null || C.FindFolder(l.FolderId.Value) == null);

            foreach (var list in SortLists(panel, rootLists))
                DrawListEntry(panel, list);

            DrawRootDropZone(panel);
        }

        private static void DrawListToolbar(ListPanelState panel)
        {
            var scale = ImGuiHelpers.GlobalScale;

            var searchWidth = panel.ReadOnly ? -1 : ImGui.GetContentRegionAvail().X - 30 * scale;
            ImGui.SetNextItemWidth(searchWidth);
            ImGui.InputTextWithHint($"##Search_{panel.Id}", "Search lists or leves...", ref panel.Search, 64);

            if (panel.ReadOnly)
                return;

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Folder, "##NewFolder"))
            {
                C.AddFolder();
                C.Save();
            }

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("New folder");

            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("Sort:");

            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            using var combo = ImRaii.Combo("##ListSort", SortModeLabel(C.SortMode));

            if (!combo.Success)
                return;

            foreach (var mode in Enum.GetValues<LeveInfo.ListSortMode>())
            {
                if (!ImGui.Selectable(SortModeLabel(mode), C.SortMode == mode))
                    continue;

                C.SortMode = mode;
                C.Save();
            }
        }

        private static void DrawFolder(ListPanelState panel, LeveInfo.SavedFolder folder, List<LeveInfo.SavedList> leveList)
        {
            using var id = ImRaii.PushId($"folder_{folder.Id}");

            using var node = ImRaii.TreeNode($"{folder.Name}###folder_{folder.Id}");

            // All of these attach to the tree node, so they must come right after it
            FolderDragSource(panel, folder);
            FolderDropTarget(panel, folder);
            DrawFolderContextMenu(panel, folder);

            if (!node.Success)
                return;

            foreach (var list in SortLists(panel, leveList.Where(l => l.FolderId == folder.Id)))
                DrawListEntry(panel, list);
        }

        private static void DrawListEntry(ListPanelState panel, LeveInfo.SavedList list)
        {
            using var id = ImRaii.PushId($"list_{panel.Id}_{list.Id}");

            if (ImGui.Selectable(list.Name, panel.SelectedId == list.Id))
                panel.SelectedId = list.Id;

            if (panel.ReadOnly)
                return;

            // Attach to the Selectable, so they must come right after it
            ListDragSource(panel, list);
            ListDropTarget(panel, list);

            if (!ImGui.BeginPopupContextItem($"##list_ctx_{list.Id}"))
                return;

            if (ImGui.MenuItem("Rename"))
            {
                _renameListId = list.Id;
                _renameFolderId = -1;
                _renameBuffer = list.Name;
                _openRenamePopup = true;
            }

            if (ImGui.MenuItem("Export"))
                ExportList(list);

            ImGui.Separator();

            if (CanReorder(panel))
            {
                if (ImGui.MenuItem("Move up"))
                {
                    C.MoveList(list, -1);
                    C.Save();
                }

                if (ImGui.MenuItem("Move down"))
                {
                    C.MoveList(list, 1);
                    C.Save();
                }

                ImGui.Separator();
            }

            if (ImGui.BeginMenu("Move to folder"))
            {
                if (ImGui.MenuItem("(None)", "", list.FolderId == null))
                {
                    C.MoveListToFolder(list, null);
                    C.Save();
                }

                foreach (var folder in SortFolders())
                {
                    if (!ImGui.MenuItem(folder.Name, "", list.FolderId == folder.Id))
                        continue;

                    C.MoveListToFolder(list, folder.Id);
                    C.Save();
                }

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }

        #endregion

        #region Menus And Popups

        private static void DrawFolderContextMenu(ListPanelState panel, LeveInfo.SavedFolder folder)
        {
            if (!ImGui.BeginPopupContextItem($"##folder_ctx_{folder.Id}"))
                return;

            if (ImGui.MenuItem("Rename"))
            {
                _renameFolderId = folder.Id;
                _renameListId = -1;
                _renameBuffer = folder.Name;
                _openRenamePopup = true;
            }

            if (ImGui.MenuItem("Export folder"))
                ExportFolder(folder);

            if (CanReorder(panel))
            {
                ImGui.Separator();

                if (ImGui.MenuItem("Move up"))
                {
                    C.MoveFolder(folder, -1);
                    C.Save();
                }

                if (ImGui.MenuItem("Move down"))
                {
                    C.MoveFolder(folder, 1);
                    C.Save();
                }
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Delete (lists move to root)"))
            {
                C.DeleteFolder(folder.Id);
                C.Save();
            }

            ImGui.EndPopup();
        }

        private static void DrawRenamePopup()
        {
            if (!ImGui.BeginPopup("RenameEntry"))
                return;

            ImGui.SetNextItemWidth(180 * ImGuiHelpers.GlobalScale);

            // Start typing right away
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();

            var submitted = ImGui.InputText("##RenameInput", ref _renameBuffer, 64,
                ImGuiInputTextFlags.EnterReturnsTrue);

            if (submitted && !string.IsNullOrWhiteSpace(_renameBuffer))
            {
                var name = _renameBuffer.Trim();

                if (_renameListId != -1)
                {
                    if (C.FindPlaylist(_renameListId) is { } list)
                        C.RenameList(list, name);
                }
                else if (C.FindFolder(_renameFolderId) is { } folder)
                {
                    folder.Name = name;
                }

                C.Save();
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        #endregion

        #region Drag And Drop

        private static void ListDragSource(ListPanelState panel, LeveInfo.SavedList list)
        {
            if (!CanReorder(panel))
                return;

            if (!ImGui.BeginDragDropSource())
                return;

            _draggedListId = list.Id;
            ImGui.SetDragDropPayload(ListPayload, BitConverter.GetBytes(list.Id));
            ImGui.TextUnformatted(list.Name);
            ImGui.EndDragDropSource();
        }

        private static void ListDropTarget(ListPanelState panel, LeveInfo.SavedList target)
        {
            if (!CanReorder(panel))
                return;

            var after = IsMouseInLowerHalf();

            if (!ImGui.BeginDragDropTarget())
                return;

            var payload = ImGui.AcceptDragDropPayload(ListPayload, LineFlags);

            // No line when hovering the list you're dragging
            if (!payload.IsNull && _draggedListId != target.Id)
            {
                DrawInsertionLine(after);

                if (payload.IsDelivery() && C.FindPlaylist(_draggedListId) is { } dragged)
                {
                    C.MoveListNextTo(dragged, target, after);
                    C.Save();
                }
            }

            ImGui.EndDragDropTarget();
        }

        private static void FolderDragSource(ListPanelState panel, LeveInfo.SavedFolder folder)
        {
            if (!CanReorder(panel))
                return;

            if (!ImGui.BeginDragDropSource())
                return;

            _draggedFolderId = folder.Id;
            ImGui.SetDragDropPayload(FolderPayload, BitConverter.GetBytes(folder.Id));
            ImGui.TextUnformatted(folder.Name);
            ImGui.EndDragDropSource();
        }

        // Folders accept lists (move inside, default highlight) and other folders (reorder, line)
        private static void FolderDropTarget(ListPanelState panel, LeveInfo.SavedFolder target)
        {
            if (!CanReorder(panel))
                return;

            var after = IsMouseInLowerHalf();

            if (!ImGui.BeginDragDropTarget())
                return;

            var listPayload = ImGui.AcceptDragDropPayload(ListPayload);
            if (!listPayload.IsNull && C.FindPlaylist(_draggedListId) is { } list)
            {
                C.MoveListToFolder(list, target.Id);
                C.Save();
            }

            var folderPayload = ImGui.AcceptDragDropPayload(FolderPayload, LineFlags);
            if (!folderPayload.IsNull && _draggedFolderId != target.Id)
            {
                DrawInsertionLine(after);

                if (folderPayload.IsDelivery() && C.FindFolder(_draggedFolderId) is { } folder)
                {
                    C.MoveFolderNextTo(folder, target, after);
                    C.Save();
                }
            }

            ImGui.EndDragDropTarget();
        }

        // Empty space at the bottom of the panel: drop a list here to move it to the root
        private static void DrawRootDropZone(ListPanelState panel)
        {
            if (!CanReorder(panel))
                return;

            var height = Math.Max(24 * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y);
            ImGui.InvisibleButton("##RootDrop", new Vector2(-1, height));

            if (!ImGui.BeginDragDropTarget())
                return;

            var payload = ImGui.AcceptDragDropPayload(ListPayload);
            if (!payload.IsNull && C.FindPlaylist(_draggedListId) is { } list)
            {
                C.MoveListToFolder(list, null);
                C.Save();
            }

            ImGui.EndDragDropTarget();
        }

        // Draws the line between items: top edge for "before", bottom edge for "after".
        // Offsetting by half the item spacing puts it in the gap, so the "after" line of
        // one item and the "before" line of the next are in the same spot.
        private static void DrawInsertionLine(bool after)
        {
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var halfGap = ImGui.GetStyle().ItemSpacing.Y * 0.5f;
            var y = after ? max.Y + halfGap : min.Y - halfGap;

            ImGui.GetWindowDrawList().AddLine(
                new Vector2(min.X, y),
                new Vector2(max.X, y),
                ImGui.GetColorU32(ImGuiCol.DragDropTarget),
                2f * ImGuiHelpers.GlobalScale);
        }

        // Screen Y grows downward, so a bigger Y means lower on screen.
        // "Mouse Y > midpoint" = the mouse is in the bottom half = drop AFTER the item.
        private static bool IsMouseInLowerHalf()
        {
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            return ImGui.GetMousePos().Y > (min.Y + max.Y) * 0.5f;
        }

        #endregion

        #region Sorting And Search

        private static IOrderedEnumerable<LeveInfo.SavedList> SortLists(ListPanelState panel, IEnumerable<LeveInfo.SavedList> lists)
        {
            // The curated panel ignores the user's sort mode, it's always by name
            var mode = panel.ReadOnly ? LeveInfo.ListSortMode.Name : C.SortMode;

            return mode switch
            {
                LeveInfo.ListSortMode.RecentlyModified => lists
                    .OrderByDescending(l => l.LastModified)
                    .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase),
                LeveInfo.ListSortMode.Manual => lists
                    .OrderBy(l => l.SortOrder)
                    .ThenBy(l => l.Name, StringComparer.OrdinalIgnoreCase),
                _ => lists.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase),
            };
        }

        private static IEnumerable<LeveInfo.SavedFolder> SortFolders()
        {
            return C.SortMode == LeveInfo.ListSortMode.Manual
                ? C.GetSortedFolders()
                : C.Leve_SavedFolders.OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string SortModeLabel(LeveInfo.ListSortMode mode) => mode switch
        {
            LeveInfo.ListSortMode.RecentlyModified => "Recently modified",
            LeveInfo.ListSortMode.Manual => "Manual",
            _ => "Name",
        };

        private static bool ListMatchesSearch(ListPanelState panel, LeveInfo.SavedList list)
        {
            if (!HasSearch(panel))
                return true;

            if (list.Name.Contains(panel.Search, StringComparison.OrdinalIgnoreCase))
                return true;

            if ((list.Description ?? "").Contains(panel.Search, StringComparison.OrdinalIgnoreCase))
                return true;

            return list.Playlist.Any(i => LeveInfo.GetLeveName(i.Id).Contains(panel.Search, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Import / Export

        private static void DrawImportExportFooter(ListPanelState panel)
        {
            var spacing = ImGui.GetStyle().ItemSpacing.X;
            var width = (ImGui.GetContentRegionAvail().X - spacing) * 0.5f;

            var selected = C.FindPlaylist(panel.SelectedId);
            var exportLabel = selected != null ? "Export" : "Export All";

            if (ImGui.Button($"{exportLabel}##ExportButton", new Vector2(width, 0)))
            {
                if (selected != null)
                    ExportList(selected);
                else
                    ExportAll();
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(selected != null
                    ? $"Copy \"{selected.Name}\" to the clipboard"
                    : "Copy every folder and list to the clipboard");
            }

            ImGui.SameLine();
            if (ImGui.Button("Import", new Vector2(width, 0)))
                ImportFromClipboard();

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Add the folders and lists copied to the clipboard");
        }

        private static void ExportAll()
        {
            if (C.Leve_SavedPlaylist.Count == 0 && C.Leve_SavedFolders.Count == 0)
            {
                ShowResult(false, "Nothing to export yet.");
                return;
            }

            ImGui.SetClipboardText(C.ExportAll());
            ShowResult(true, "Copied all folders and lists to the clipboard.");
        }

        private static void ExportList(LeveInfo.SavedList list)
        {
            ImGui.SetClipboardText(C.Export(Array.Empty<LeveInfo.SavedFolder>(), new[] { list }));
            ShowResult(true, $"Copied \"{list.Name}\" to the clipboard.");
        }

        private static void ExportFolder(LeveInfo.SavedFolder folder)
        {
            var lists = C.Leve_SavedPlaylist.Where(l => l.FolderId == folder.Id).ToList();

            ImGui.SetClipboardText(C.Export(new[] { folder }, lists));
            ShowResult(true, $"Copied folder \"{folder.Name}\" and its {lists.Count} list(s) to the clipboard.");
        }

        private static void ImportFromClipboard()
        {
            var result = C.Import(ImGui.GetClipboardText());
            if (result.Success)
                C.Save();

            ShowResult(result.Success, result.Message);
        }

        private static void ShowResult(bool success, string message)
        {
            if (success)
                Notify.Success(message);
            else
                Notify.Error(message);
        }

        #endregion

        #endregion

        #region Playlist Details

        private static bool _editName = false;
        private static string _newName = string.Empty;

        private static bool _editDescription = false;
        private static string _descriptionInfo = string.Empty;

        // Which list the edit buffers belong to, so switching lists mid-edit can't write into the wrong one
        private static int _editListId = -1;

        private static void LevePlaylistDetails(ListPanelState panel, List<LeveInfo.SavedList> list)
        {
            var allowEdit = !panel.ReadOnly;

            using (var listInfo = ImRaii.Child($"Leve Playlist: Details_{panel.Id}", ImGui.GetContentRegionAvail(), true))
            {
                if (!listInfo.Success)
                    return;

                var savedList = list.Find(c => c.Id == panel.SelectedId);
                if (savedList == null)
                    return;

                if (_editListId != savedList.Id)
                {
                    _editListId = savedList.Id;
                    _editName = false;
                    _editDescription = false;
                }

                if (ImGui.Button("Overwrite current list"))
                {
                    OverwriteList(savedList.Playlist);
                }
                ImGui.SameLine();
                if (ImGui.Button("Add to current list"))
                {
                    AddToCurrent(savedList.Playlist);
                }

                if (allowEdit)
                {
                    ImGui.SameLine();
                    var keysHeld = ImGui.IsKeyDown(ImGuiKey.LeftShift) || ImGui.IsKeyDown(ImGuiKey.RightShift);

                    using (ImRaii.Disabled(!keysHeld))
                    {
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Delete List"))
                        {
                            C.Leve_SavedPlaylist.Remove(savedList);
                            C.SaveDebounced();

                            panel.SelectedId = 0;
                        }
                    }
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Remove the playlist.\n" +
                            "Hold shift to allow");
                    }
                }

                if (_editName)
                {
                    if (ImGui.Button("Save New Name##Name"))
                    {
                        C.RenameList(savedList, _newName);
                        C.Save();

                        _newName = string.Empty;
                        _editName = false;
                    }
                    ImGui.SameLine();
                }
                else if (allowEdit)
                {
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.PencilAlt, "Edit List Name"))
                    {
                        _newName = savedList.Name;
                        _editName = true;
                    }
                    ImGui.SameLine();
                }

                if (_editDescription)
                {
                    if (ImGui.Button("Save##Description"))
                    {
                        savedList.Description = _descriptionInfo;
                        savedList.Touch();
                        C.Save();

                        _descriptionInfo = string.Empty;
                        _editDescription = false;
                    }
                }
                else if (allowEdit)
                {
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.PencilAlt, "Edit Description"))
                    {
                        _descriptionInfo = savedList.Description;
                        _editDescription = true;
                    }
                }

                if (_editName)
                {
                    ImGui.InputText("Name", ref _newName, 100);
                }
                else
                {
                    ImGui.Text($"List: {savedList.Name}");
                }

                if (_editDescription)
                {
                    ImGui.InputTextMultiline("Description", ref _descriptionInfo, 3000, new Vector2(-1, 100));
                }
                else
                {
                    ImGui.Text($"Description:");
                    ImGui.Text($"{savedList.Description}");
                }

                PlaylistTable(savedList.Playlist);
            }
        }

        private static void PlaylistTable(List<LeveInfo.Info_List> leveList)
        {
            using (var table = ImRaii.Table("Leve Saved Playlist: Table", 5, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.ScrollY))
            {
                if (!table.Success)
                    return;

                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Level");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Amount");

                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                foreach (var leve in leveList)
                {
                    if (!LeveInfo.Leve_SheetInfo.TryGetValue(leve.Id, out var sheetInfo))
                        continue;

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{leve.Id}");

                    ImGui.TableNextColumn();
                    GameIcons.DrawInline(LeveInfo.Assignment_IconDict[sheetInfo.JobAssignmentType].IconId, false);

                    ImGui.TableNextColumn();
                    ImGui.Text($"{sheetInfo.Level}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{sheetInfo.LeveName}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{leve.Amount}");
                }
            }
        }

        private static void OverwriteList(List<LeveInfo.Info_List> leveList)
        {
            C.LeveOrder.Clear();
            foreach (var leve in leveList)
            {
                C.LeveOrder.Add(leve.Id);
                C.LeveList[leve.Id] = leve.Amount;
            }
            C.Save();
        }

        private static void AddToCurrent(List<LeveInfo.Info_List> leveList)
        {
            foreach (var leve in leveList)
            {
                if (!C.LeveOrder.Contains(leve.Id))
                    C.LeveOrder.Add(leve.Id);

                C.LeveList[leve.Id] = leve.Amount;
            }
            C.Save();
        }

        #endregion
    }
}