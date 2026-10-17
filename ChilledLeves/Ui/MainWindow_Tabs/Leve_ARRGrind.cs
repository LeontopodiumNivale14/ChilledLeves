using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Scheduler;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_ARRGrind
    {

        // TODO IDEAS:
        // Playlist mode where you can set which NPC to run till X thing
        // Would allow for making sure all leves are completed at each vendor as ex.
        // Or would allow for leveling till X at vendor A, then leveling till Y at vendor B
        // Still need to curate a list of npcs for this though that this will work with. . . 

        private static readonly List<uint> ValidNpcs = new()
        {
            1000970, // T'mookri
            1000101, // Gontrant
            1004342, // Wyrkholsk
            1004344, // Nahctahr
            1001788, // Swygskyf
            1001791, // Orwen
            1003888, // Graceful Song
            1001796, // Totonowa
            1001799, // Poponagu
            1000105, // Tierney
            1001866, // Muriaule
            1000821, // Qina Lyehga
            1000823, // Nyell
            1002384, // Cimeaurant
            1002401, // Voilinaut
            1004348, // K'leytai
            1011208, // Eloin 50-58
        };

        private static List<AssignmentType> validAssignments = new() { AssignmentType.Miner, AssignmentType.Botanist, AssignmentType.Fisher };
        private static int selectedOption = 0;
        private static List<string> SortOptions = new() { "Level [High -> Low]", "Level [Low -> High]", "Not Completed", "Completed", "ID" };
        private static float _npcInfoHeight = 100f;
        private static readonly ImGuiEx.RealtimeDragDrop<uint> _leveDragDrop = new("LeveOrder", id => id.ToString());

        public static void Draw()
        {
            using (var child = ImRaii.Child("ARR Grind: NPC Mode", default, false))
            {
                if (!child.Success)
                    return;

                var selectedNpc = C.ARR_NpcId;
                var selectedJob = C.ARR_SelectedAssignment;

                var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

                var rightColumnAvail = ImGui.GetContentRegionAvail();
                float availWidth = rightColumnAvail.X;

                using (var ARR_NpcWindow = ImRaii.Child("ARR: Npc Info", new(availWidth, _npcInfoHeight), true))
                {
                    if (!ARR_NpcWindow.Success)
                        return;

                    // TODO: actually wire this back up when i fix this whole mode
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "Start ARR Grind", Leve_Helper.IsIdle))
                    {
                        Leve_Helper.SelectedMode = ModeSelection.ARR_Grind;
                        Leve_Helper.State = LeveState.CheckLeves;
                    }
                    ImGui.SameLine();
                    if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Square, "Stop", Leve_Helper.State != LeveState.Idle))
                    {
                        SchedulerMain.DisablePlugin();
                    }

                    if (LeveInfo.Levemete_Info.TryGetValue(selectedNpc, out var npcInfo))
                    {
                        using (var table = ImRaii.Table("Settings", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                        {
                            if (!table.Success)
                                return;

                            // NPC Selection
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Button("Select another Levemete"))
                            {
                                ImGui.OpenPopup("Chilled Leves: ARR NPC Selection");
                            }

                            NpcSelectionWindow();

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Current Letemete:");

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{npcInfo.Name}");
                            ImGui.SameLine();
                            ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, npcInfo.TerritoryName(), false);

                            // Job Selection
                            ImGui.TableNextRow();
                            ImGui.AlignTextToFramePadding();
                            ImGui.TableSetColumnIndex(0);
                            for (int i = 0; i < validAssignments.Count(); i++)
                            {
                                var type = validAssignments[i];
                                if (i != 0)
                                    ImGui.SameLine();
                                JobToggleButton(type);
                            }

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text("Selected Job:");

                            ImGui.TableNextColumn();
                            var className = LeveInfo.Assignment_IconDict[selectedJob].Name;
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{className}");

                            // Selected mode
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Button("Select Mode"))
                            {
                                ImGui.OpenPopup("ARR Grind: Select Mode");
                            }
                            ARR_ModeSelect();

                            ImGui.TableNextColumn();
                            var mode = C.ARR_StopCondition;
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"Stop when: {SelectedMode(mode)}");

                            ImGui.TableNextColumn();
                            if (mode is StopConditions.Level)
                            {
                                ImGui.SetNextItemWidth(200);
                                var arr_stopLv = C.ARR_StopLevel;
                                if (ImGui.InputUInt("Lv.", ref arr_stopLv, 0, 10))
                                {
                                    C.ARR_StopLevel = arr_stopLv;
                                    C.SaveDebounced();
                                }
                            }

                            // Sort list buttons
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Button("Sort List"))
                            {
                                var fullList = C.ARR_LevemetPriority[selectedNpc];

                                SortInPlace(fullList, x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == selectedJob, selectedOption);
                                C.Save();
                            }

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text("Sort list by...");

                            ImGui.TableNextColumn();
                            ImGui.SetNextItemWidth(CalcComboWidth(SortOptions));
                            using (var combo = ImRaii.Combo("##SorterOption", SortOptions[selectedOption]))
                            {
                                if (combo.Success)
                                {
                                    for (int i = 0; i < SortOptions.Count; i++)
                                    {
                                        var isSelected = i == selectedOption;
                                        var option = SortOptions[i];
                                        if (ImGui.Selectable(option, isSelected))
                                        {
                                            selectedOption = i;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    _npcInfoHeight = ImGui.GetCursorPosY() + ImGui.GetStyle().WindowPadding.Y;
                }

                // actually show the npc details here if it's valid. Need to check that
                using (var ARR_LeveInfo = ImRaii.Child("ARR: Leve Info", ImGui.GetContentRegionAvail(), true))
                {
                    if (!ARR_LeveInfo.Success)
                        return;

                    if (LeveInfo.Levemete_Info.TryGetValue(selectedNpc, out var vendorInfo))
                    {
                        var leveList = vendorInfo.Leves.Where(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == selectedJob).ToList();

                        if (leveList.Count() != 0)
                        {

                            _leveDragDrop.Begin();
                            using (var table = ImRaii.Table("Levemete: Table Info", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
                            {
                                if (!table.Success)
                                    return;

                                ImGui.TableSetupColumn("##Reorder");
                                ImGui.TableSetupColumn("ID");
                                ImGui.TableSetupColumn("Job");
                                ImGui.TableSetupColumn("Status");
                                ImGui.TableSetupColumn("Lv.");
                                ImGui.TableSetupColumn("Name");
                                ImGui.TableSetupColumn("Need");
                                ImGui.TableSetupColumn("Have");

                                ImGui.TableHeadersRow();

                                var fullList = C.ARR_LevemetPriority[selectedNpc];
                                var validLeves = fullList.Where(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == selectedJob).ToList();

                                for (int i = 0; i < validLeves.Count(); i++)
                                {
                                    var leve = validLeves[i];
                                    var sheetInfo = LeveInfo.Leve_SheetInfo[leve];
                                    ImGui.PushID($"{leve}_{sheetInfo.LeveName}");

                                    ImGui.TableNextRow();
                                    _leveDragDrop.NextRow();
                                    _leveDragDrop.SetRowColor(leve.ToString());

                                    ImGui.TableSetColumnIndex(0);
                                    var realIndex = fullList.IndexOf(leve);
                                    _leveDragDrop.DrawButtonDummy(leve.ToString(), fullList, realIndex);

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{leve}");

                                    ImGui.TableNextColumn();
                                    GameIcons.DrawInlineOrIcon(LeveInfo.Assignment_IconDict[sheetInfo.JobAssignmentType].IconId, FontAwesomeIcon.Book);

                                    ImGui.TableNextColumn();
                                    uint statusId = Utils.Leve_IsComplete(leve) ? LeveInfo.Leve_State[Leve_Status.NotComplete] : LeveInfo.Leve_State[Leve_Status.Complete];
                                    ImGui_Ice.ImageButtonWithText(statusId, $"{sheetInfo.AllowanceCost}", sheetInfo.LeveName, sidePadding: 4);

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{sheetInfo.Level}");

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{sheetInfo.LeveName}");

                                    var currentAmount = 0;
                                    var neededAmount = 0;

                                    if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                                    {
                                        var materialInfo = sheetInfo.MaterialInfo;
                                        var itemId = materialInfo.Item_Id;

                                        currentAmount = Utils.GetItemCount(itemId);
                                        neededAmount = Utils.Leve_RequiredAmount(materialInfo);

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

                                    ImGui.PopID();
                                }
                            }

                            _leveDragDrop.End();
                        }
                        else
                        {
                            ImGui.Text("This leve doesn't currently have info for any leves under this job. Please select a different one");
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        private static void NpcSelectionWindow()
        {
            using (var popup = ImRaii.Popup("Chilled Leves: ARR NPC Selection"))
            {
                if (!popup.Success)
                    return;

                ImGui.Text($"Select your ARR levemete");

                ImGui.Dummy(new(0, 5));

                using (var table = ImRaii.Table("Npc Selection Info", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                {
                    if (!table.Success)
                        return;

                    var iconSize = new Vector2(ImGui.GetTextLineHeight());
                    var iconColumnWidth = iconSize.X + ImGui.GetStyle().CellPadding.X * 2f;
                    var scale = ImGuiHelpers.GlobalScale;

                    ImGui.TableSetupColumn("##Selected", ImGuiTableColumnFlags.WidthFixed, 5 * scale);
                    ImGui.TableSetupColumn("Npc");
                    ImGui.TableSetupColumn("Location");
                    ImGui.TableSetupColumn("Jobs", ImGuiTableColumnFlags.WidthStretch);

                    ImGui.TableHeadersRow();

                    var selectedNpc = C.ARR_NpcId;

                    foreach (var npc in ValidNpcs)
                    {
                        if (LeveInfo.Levemete_Info.TryGetValue(npc, out var npcInfo))
                        {
                            bool isSelected = selectedNpc == npc;

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (isSelected)
                            {
                                var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;

                                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
                            }

                            ImGui.TableNextColumn();
                            ImGui.AlignTextToFramePadding();
                            bool clicked = RowSelectable($"{npcInfo.Name}", isSelected);

                            if (clicked)
                            {
                                C.ARR_NpcId = npc;
                                C.Save();
                                ImGui.CloseCurrentPopup();
                            }

                            ImGui.TableNextColumn();
                            var territory = ExcelHelper.Sheet_TerritoryType.GetRow(npcInfo.TerritoryId).PlaceName.Value.Name.ToString();
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text($"{territory}");

                            ImGui.TableNextColumn();
                            bool useSameLine = false;
                            for (int i = 0; i < validAssignments.Count(); i++)
                            {
                                var assignment = validAssignments[i];
                                var leveList = npcInfo.Leves.Where(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == assignment).ToList();
                                if (leveList.Count > 0)
                                {
                                    if (useSameLine)
                                        ImGui.SameLine();

                                    DrawIcon(assignment, iconSize);
                                    if (!useSameLine)
                                        useSameLine = true;

                                    if (ImGui.IsItemHovered())
                                    {
                                        var levelsText = leveList
                                            .Select(leve => LeveInfo.Leve_SheetInfo[leve].Level)
                                            .Distinct()
                                            .OrderBy(level => level)
                                            .Select(level => level.ToString())
                                            .Aggregate((a, b) => $"{a}, {b}");

                                        ImGui.SetTooltip($"Leve Levels: {levelsText}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ARR_ModeSelect()
        {
            using (var selectModePopup = ImRaii.Popup("ARR Grind: Select Mode"))
            {
                if (!selectModePopup.Success)
                    return;

                var level = StopConditions.Level;
                var complete = StopConditions.Complete;
                var noAllowance = StopConditions.NoAllowance;

                var currentStopMode = C.ARR_StopCondition;

                if (ImGui.RadioButton(SelectedMode(level), currentStopMode == level))
                {
                    C.ARR_StopCondition = level;
                    C.Save();
                    ImGui.CloseCurrentPopup();
                }
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "Run this levemete until you hit a particular level on the selected class");


                if (ImGui.RadioButton(SelectedMode(complete), currentStopMode == complete))
                {
                    C.ARR_StopCondition = complete;
                    C.Save();
                    ImGui.CloseCurrentPopup();
                }
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "Run this levemete until all leves are counted from this npc");

                if (ImGui.RadioButton(SelectedMode(noAllowance), currentStopMode == noAllowance))
                {
                    C.ARR_StopCondition = noAllowance;
                    C.Save();
                    ImGui.CloseCurrentPopup();
                }
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, "Run this levemete until you have no more allowance to do so");
            }
        }

        private static void DrawIcon(AssignmentType type, Vector2 iconSize)
        {
            var iconId = LeveInfo.Assignment_IconDict[type].IconId;
            GameIcons.DrawInline(iconId);
        }

        private static bool RowSelectable(string id, bool isSelected, float height = 0f)
        {
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));

            bool clicked = ImGui.Selectable(id, isSelected,
                ImGuiSelectableFlags.SpanAllColumns, new Vector2(0, height));

            ImGui.PopStyleColor(3);

            if (isSelected)
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderActive));
            else if (ImGui.IsItemHovered())
                ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderHovered));

            return clicked;
        }

        private static void JobToggleButton(AssignmentType selectedClass)
        {
            bool enabled = C.ARR_SelectedAssignment == selectedClass;
            var iconId = LeveInfo.Assignment_IconDict[selectedClass].IconId;

            // Resolve colors up front based on theme + enabled state
            Vector4 bgColor;
            Vector4 borderColor;
            float borderThickness;
            bool grey;

            if (enabled)
            {
                if (C.UseIceTheme)
                {
                    bgColor = Theme_Colors.LightSlate;
                    borderColor = Theme_Colors.IceBlue;
                }
                else
                {
                    bgColor = new Vector4(0.3f, 0.3f, 0.35f, 0.7f);
                    borderColor = ImGuiColors.ParsedGold;
                }
                borderThickness = 1.0f;
                grey = false;
            }
            else if (C.UseIceTheme)
            {
                bgColor = Theme_Colors.DarkSlate;
                borderColor = Theme_Colors.TranslucentIce;
                borderThickness = 0.5f;
                grey = true;
            }
            else
            {
                bgColor = new Vector4(0.2f, 0.2f, 0.2f, 0.1f);
                borderColor = new Vector4(0.4f, 0.4f, 0.4f, 0.5f);
                borderThickness = 0.5f;
                grey = true;
            }

            var size = new Vector2(ImGui.GetFrameHeight());
            if (!GameIcons.TryGetScaledIcon(iconId, (int)size.Y, out var texture, grey))
                return;

            bool clicked = IconOnlyToggleButton(texture, $"jobtoggle_{selectedClass}", size, bgColor, borderColor, borderThickness);

            if (clicked)
            {
                C.ARR_SelectedAssignment = selectedClass;
                C.SaveDebounced();
            }
        }

        private static bool IconOnlyToggleButton(IDalamudTextureWrap texture, string id, Vector2 size, Vector4 bgColor, Vector4 borderColor, float borderThickness)
        {
            var pos = ImGui.GetCursorScreenPos();
            bool clicked = ImGui.InvisibleButton($"##{id}", size);

            bool hovered = ImGui.IsItemHovered();
            bool active = ImGui.IsItemActive();

            var drawList = ImGui.GetWindowDrawList();
            var rounding = ImGui.GetStyle().FrameRounding;

            // Transparent background at rest; still show hover/active feedback
            uint fillColor = active
                ? ImGui.GetColorU32(bgColor * new Vector4(1.3f, 1.3f, 1.3f, 1f))
                : hovered
                    ? ImGui.GetColorU32(bgColor * new Vector4(1.15f, 1.15f, 1.15f, 1f))
                    : ImGui.GetColorU32(bgColor);

            drawList.AddRectFilled(pos, pos + size, fillColor, rounding);
            drawList.AddRect(pos, pos + size, ImGui.GetColorU32(borderColor), rounding, ImDrawFlags.None, borderThickness);

            drawList.AddImage(texture.Handle, pos, pos + size, Vector2.Zero, Vector2.One);

            return clicked;
        }

        private static string SelectedMode(StopConditions stopCondition)
        {
            return stopCondition switch
            {
                StopConditions.Level => "Level",
                StopConditions.Complete => "Completed All",
                StopConditions.NoAllowance => "Allowance is 0",
                _ => "???",
            };
        }
        private static float CalcComboWidth(IEnumerable<string> options)
        {
            var style = ImGui.GetStyle();
            var maxTextWidth = 0f;

            foreach (var option in options)
            {
                var width = ImGui.CalcTextSize(option).X;
                if (width > maxTextWidth)
                    maxTextWidth = width;
            }

            // text + left/right frame padding + the arrow button (which is a square of frame height)
            return maxTextWidth + (style.FramePadding.X * 2f) + ImGui.GetFrameHeight();
        }
        private static void SortInPlace(List<uint> fullList, Func<uint, bool> belongsToGroup, int option)
        {
            var slots = new List<int>();
            var group = new List<uint>();

            for (int i = 0; i < fullList.Count; i++)
            {
                if (!belongsToGroup(fullList[i]))
                    continue;

                slots.Add(i);
                group.Add(fullList[i]);
            }

            if (group.Count < 2)
                return;

            var sorted = SortLeves(group, option);

            for (int i = 0; i < slots.Count; i++)
                fullList[slots[i]] = sorted[i];
        }
        private static List<uint> SortLeves(List<uint> leves, int option)
        {
            if (leves.Count == 0)
                return leves;

            return option switch
            {
                // Level [High -> Low]
                0 => leves.OrderBy(GetAllowance).ThenByDescending(GetLevel).ThenBy(x => x).ToList(),

                // Level [Low -> High]
                1 => leves.OrderBy(GetAllowance).ThenBy(GetLevel).ThenBy(x => x).ToList(),

                // Not Completed first (false sorts before true)
                2 => leves.OrderBy(GetAllowance).ThenBy(Utils.Leve_IsComplete).ThenByDescending(GetLevel).ThenBy(x => x).ToList(),

                // Completed first (true sorts first when descending)
                3 => leves.OrderBy(GetAllowance).ThenByDescending(Utils.Leve_IsComplete).ThenByDescending(GetLevel).ThenBy(x => x).ToList(),

                // ID
                4 => leves.OrderBy(GetAllowance).ThenBy(x => x).ToList(),

                _ => leves,
            };
        }


        private static uint GetLevel(uint leveId)
            => LeveInfo.Leve_SheetInfo[leveId].Level; // swap in whatever your sheet info calls the level

        private static int GetAllowance(uint leveId)
            => LeveInfo.Leve_SheetInfo[leveId].AllowanceCost;
    }
}
