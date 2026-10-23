using ChilledLeves.Gui;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;
using static ChilledLeves.Config_Files.Config;
using static FFXIVClientStructs.FFXIV.Client.UI.UIModule.Delegates;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_List
    {
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

        private static int _selectedListId = 0;

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

                    var scale = ImGuiHelpers.GlobalScale;

                    LeveSelectionWindow(C.Leve_SavedPlaylist);
                    ImGui.SameLine();
                    LevePlaylistDetails(C.Leve_SavedPlaylist, true);
                }
            }
        }

        private static void LevePacks()
        {
            using (var levepackTab = ImRaii.TabItem("Currated List"))
            {
                if (!levepackTab.Success)
                    return;

                using (var child = ImRaii.Child("Personal List Tab Details", default))
                {
                    if (!child.Success)
                        return;

                    LeveSelectionWindow(LeveInfo.Curated_Levepacks);
                    ImGui.SameLine();
                    LevePlaylistDetails(LeveInfo.Curated_Levepacks, false);
                }
            }
        }

        private static void LeveSelectionWindow(List<LeveInfo.SavedList> leveList)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var selectionWindow = new Vector2(200 * scale, ImGui.GetContentRegionAvail().Y);

            using (var listSelect = ImRaii.Child("Leve Selection Window", selectionWindow, true))
            {
                if (!listSelect.Success)
                    return;

                foreach (var list in leveList)
                {
                    bool isSelected = _selectedListId == list.Id;
                    ImGui.PushID($"{list.Name}_{list.Id}");

                    if (ImGui.Selectable($"{list.Name}", isSelected))
                    {
                        _selectedListId = list.Id;
                    }
                }
            }
        }

        private static bool _editName = false;
        private static string _newName = string.Empty;

        private static bool _editDescription = false;
        private static string _descriptionInfo = string.Empty;

        private static void LevePlaylistDetails(List<LeveInfo.SavedList> list, bool allowEdit)
        {
            using (var listInfo = ImRaii.Child("Leve Playlist: Details", ImGui.GetContentRegionAvail(), true))
            {
                if (!listInfo.Success)
                    return;

                var savedList = list.Find(c => c.Id == _selectedListId);
                if (savedList == null)
                    return;

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

                            _selectedListId = 0;
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
                        savedList.Name = _newName;
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
                    if (LeveInfo.Leve_SheetInfo.TryGetValue(leve.Id, out var sheetInfo))
                    {
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
        }
    }
}
