using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Scheduler;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using SharpDX.Direct2D1.Effects;
using System.Collections.Generic;
using static ChilledLeves.Utilities.LeveData.LeveInfo;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_Playlist
    {
        private static readonly ImGuiEx.RealtimeDragDrop<uint> _leveDragDrop = new("LeveOrder", id => id.ToString());
        private static float _npcInfoHeight = 100f;

        public static void Draw()
        {
            using (var child = ImRaii.Child("Worklist: Child Window", new(-1, -1)))
            {
                if (!child.Success)
                    return;

                using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

                // Top block: name + job select + mission type checkboxes
                // It's currently set to be 4.4 because that fits it + scales really nicely
                // If a user mentions something about it, I should just bump it to like 4.8

                var rightColumnAvail = ImGui.GetContentRegionAvail();
                float availWidth = rightColumnAvail.X;

                using (var child_Buttons = ImRaii.Child("Worklist: Buttons", new(availWidth, _npcInfoHeight), true))
                {
                    if (!child_Buttons.Success)
                        return;

                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "Start Playlist", Leve_Helper.State == LeveState.Idle))
                    {
                        Leve_Helper.SelectedMode = ModeSelection.Standard;
                        Leve_Helper.State = LeveState.CheckLeves;
                    }
                    ImGui.SameLine();
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Square, "Stop", Leve_Helper.State != LeveState.Idle))
                    {
                        SchedulerMain.DisablePlugin();
                    }

                    bool allowGrabMulti = C.GrabMulti;
                    if (ImGui.Checkbox("Allow grabbing multiple leves", ref allowGrabMulti))
                    {
                        C.GrabMulti = allowGrabMulti;
                        C.Save();
                    }

                    bool allowMultiTurnin = C.AllowMultiTurnin;
                    if (ImGui.Checkbox("Allow Multi-Turnin Leves", ref allowMultiTurnin))
                    {
                        C.AllowMultiTurnin = allowMultiTurnin;
                        C.Save();
                    }
                    ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                        "If a leve has multiple turnin, this will allow you to do said multiple turnins of the leve.\n" +
                        "This really only applies for specific leves below Lv. 80, as post that they stopped doing this.\n" +
                        "Enabling this will update the counts of leves that allow it");

                    if (ImGui.Button("Add Other Leves"))
                    {
                        ImGui.OpenPopup("Playlist: Leve Search");
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Save Playlist"))
                    {
                        _listName = string.Empty;
                        _listDescription = string.Empty;
                        ImGui.OpenPopup("Save New Playlist");
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Show Recipe Info"))
                    {
                        P.window_CraftDetails.IsOpen = true;
                    }

                    Popup_LeveSearch();
                    SavePopup();

                    _npcInfoHeight = ImGui.GetCursorPosY() + ImGui.GetStyle().WindowPadding.Y;


                }

                using (var child_LeveTable = ImRaii.Child("Worklist: Leve Table", ImGui.GetContentRegionAvail(), true))
                {
                    if (!child_LeveTable.Success)
                        return;

                    List<uint> levesToRemove = new();

                    _leveDragDrop.Begin();

                    if (C.LeveOrder.Count != 0)
                    {
                        if (ImGui.BeginTable("Leve Selection", 8, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                        {
                            ImGui.TableSetupColumn("##drag");
                            ImGui.TableSetupColumn("Id");
                            ImGui.TableSetupColumn("Job");
                            ImGui.TableSetupColumn("Name");
                            ImGui.TableSetupColumn("Run Amount");
                            ImGui.TableSetupColumn("Need");
                            ImGui.TableSetupColumn("Have");
                            ImGui.TableSetupColumn("Remove");

                            ImGui.TableHeadersRow();

                            for (var i = 0; i < C.LeveOrder.Count; i++)
                            {
                                var leve = C.LeveOrder[i];
                                var sheetInfo = LeveInfo.Leve_SheetInfo[leve];
                                ImGui.PushID($"{leve}_{sheetInfo.LeveName}");

                                ImGui.TableNextRow();
                                _leveDragDrop.NextRow();
                                _leveDragDrop.SetRowColor(leve.ToString());

                                ImGui.TableSetColumnIndex(0);
                                _leveDragDrop.DrawButtonDummy(leve.ToString(), C.LeveOrder, i);

                                ImGui.TableNextColumn();
                                ImGui.AlignTextToFramePadding();
                                ImGui.Text($"{leve}");

                                ImGui.TableNextColumn();
                                GameIcons.DrawInlineOrIcon(LeveInfo.Assignment_IconDict[sheetInfo.JobAssignmentType].IconId, FontAwesomeIcon.Book);

                                ImGui.TableNextColumn();
                                ImGui.AlignTextToFramePadding();
                                Utils.ShowWarning(leve);
                                ImGui.Text($"{sheetInfo.LeveName}");

                                ImGui.TableNextColumn();
                                var runAmount = C.LeveList[leve];
                                ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
                                if (ImGui.InputInt("##RunAmount", ref runAmount, 1, 10))
                                {
                                    if (runAmount < 0)
                                        runAmount = 0;

                                    C.LeveList[leve] = runAmount;
                                    C.SaveDebounced();
                                }

                                var currentAmount = 0;
                                var neededAmount = 0;

                                if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                                {
                                    var materialInfo = sheetInfo.MaterialInfo;
                                    var itemId = materialInfo.Item_Id;

                                    currentAmount = Utils.GetItemCount(itemId);
                                    neededAmount = Utils.Leve_RequiredAmount(materialInfo) * runAmount;

                                    ImGui.TableNextColumn();
                                    ImGui_Ice.ImageButtonWithText(materialInfo.IconId, $"{neededAmount}", "NeedAmount");
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip($"{materialInfo.Item_Name}");
                                    }

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{currentAmount}");
                                }
                                else
                                {
                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"-");

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"-");
                                }

                                ImGui.TableNextColumn();
                                if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                                {
                                    levesToRemove.Add(leve);
                                }

                                ImGui.PopID();
                            }

                            ImGui.EndTable();
                        }

                        foreach (var leve in levesToRemove)
                        {
                            C.LeveOrder.Remove(leve);
                            C.Save();
                        }
                    }
                    else
                    {
                        ImGui.Text("We don't have any leves currently added, please add some from the Leve Selection Tab");
                    }

                    _leveDragDrop.End();
                }
            }
        }

        private static string _leveNameSearch = "";
        private static string _leveNameSearchLast = null;
        private static List<KeyValuePair<uint, Info_LeveSheetData>> _leveSearchFiltered = new();
        private static int _leveSearchPage = 0;
        private const int LeveSearchPageSize = 20;

        private static void Popup_LeveSearch()
        {
            using var popup = ImRaii.Popup("Playlist: Leve Search");
            if (!popup.Success)
                return;

            ImGui.SetNextItemWidth(300f);
            ImGui.InputTextWithHint("##LeveNameSearch", "Search leve name/item...", ref _leveNameSearch, 128);

            RefreshLeveSearchIfNeeded();

            var totalPages = Math.Max(1, (int)Math.Ceiling(_leveSearchFiltered.Count / (float)LeveSearchPageSize));
            _leveSearchPage = Math.Clamp(_leveSearchPage, 0, totalPages - 1);

            DrawLeveSearchTable();
            DrawLeveSearchPager(totalPages);
        }

        private static void RefreshLeveSearchIfNeeded()
        {
            if (_leveNameSearchLast == _leveNameSearch)
                return;

            _leveNameSearchLast = _leveNameSearch;
            _leveSearchPage = 0;

            if (string.IsNullOrWhiteSpace(_leveNameSearch))
            {
                _leveSearchFiltered = Leve_SheetInfo.ToList();
                return;
            }

            _leveSearchFiltered = Leve_SheetInfo
                .Where(kv =>
                    kv.Value.LeveName.Contains(_leveNameSearch, StringComparison.OrdinalIgnoreCase) ||
                    kv.Value.MaterialInfo?.Item_Name?.Contains(_leveNameSearch, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        private static void DrawLeveSearchTable()
        {
            using var table = ImRaii.Table("Playlist: Leve Search Table", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg);
            if (!table.Success)
                return;

            ImGui.TableSetupColumn("##Added", ImGuiTableColumnFlags.WidthFixed, 5);
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Job");
            ImGui.TableSetupColumn("Lv");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Item");
            ImGui.TableHeadersRow();

            var start = _leveSearchPage * LeveSearchPageSize;
            var end = Math.Min(start + LeveSearchPageSize, _leveSearchFiltered.Count);

            for (var i = start; i < end; i++)
            {
                var (leveId, sheetInfo) = _leveSearchFiltered[i];

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                bool isAdded = C.LeveOrder.Contains(leveId);
                if (isAdded)
                {
                    var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;
                    ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
                }

                ImGui.TableNextColumn();
                float scale = ImGuiHelpers.GlobalScale;
                float rowHeight = 25 * scale;
                ImGui.AlignTextToFramePadding();
                bool clicked = RowSelectable(leveId.ToString(), false);
                if (clicked)
                {
                    if (!C.LeveOrder.Contains(leveId))
                    {
                        C.LeveOrder.Add(leveId);

                        if (C.LeveList[leveId] == 0)
                            C.LeveList[leveId] = 1;

                        C.SaveDebounced();
                    }
                    ImGui.CloseCurrentPopup();
                }

                ImGui.TableNextColumn();
                GameIcons.DrawInline(LeveInfo.Assignment_IconDict[sheetInfo.JobAssignmentType].IconId);

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(sheetInfo.Level.ToString());

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Utils.ShowWarning(leveId);
                ImGui.TextUnformatted(sheetInfo.LeveName);

                ImGui.TableNextColumn();
                if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                {
                    var neededAmount = Utils.Leve_RequiredAmount(sheetInfo.MaterialInfo);
                    ImGui_Ice.ImageButtonWithText(sheetInfo.MaterialInfo.IconId, neededAmount.ToString(), neededAmount.ToString());
                }
                else
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted("-");
                }
            }
        }

        private static void DrawLeveSearchPager(int totalPages)
        {
            ImGui.BeginDisabled(_leveSearchPage <= 0);
            if (ImGui.Button("< Prev"))
                _leveSearchPage--;
            ImGui.EndDisabled();

            ImGui.SameLine();
            ImGui.TextUnformatted($"Page {_leveSearchPage + 1} / {totalPages} ({_leveSearchFiltered.Count} results)");
            ImGui.SameLine();

            ImGui.BeginDisabled(_leveSearchPage >= totalPages - 1);
            if (ImGui.Button("Next >"))
                _leveSearchPage++;
            ImGui.EndDisabled();
        }

        public static bool RowSelectable(string id, bool isSelected)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));

            bool clicked = ImGui.Selectable(id, isSelected,
                ImGuiSelectableFlags.SpanAllColumns, new Vector2(0));

            ImGui.PopStyleColor(3);

            if (isSelected)
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderActive));
            else if (ImGui.IsItemHovered())
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderHovered));

            return clicked;
        }

        private static string _listName = string.Empty;
        private static string _listDescription = string.Empty;
        public static void SavePopup()
        {
            using (var popup = ImRaii.Popup("Save New Playlist"))
            {
                if (!popup.Success)
                    return;

                ImGui.InputText("Profile Name", ref _listName);
                ImGui.InputTextMultiline("Description", ref _listDescription, 3000);
                using (ImRaii.Disabled(_listName == string.Empty))
                {
                    if (ImGui.Button("Save New Playlist"))
                    {
                        C.SaveNewPlaylist(_listName, _listDescription);
                        ImGui.CloseCurrentPopup();
                    }
                }
            }
        }
    }
}
