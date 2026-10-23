using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using System.Collections.Generic;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Levemetes_Info
    {
        private static List<KeyValuePair<uint, LeveInfo.Info_Vendor>> _validNpcs = new();
        private static uint _selectedNpc = 0;
        private static AssignmentType _selectedType = AssignmentType.None;
        private static readonly List<AssignmentType> _jobOrder = new()
        {
            AssignmentType.None,

            AssignmentType.Carpenter, AssignmentType.Blacksmith,
            AssignmentType.Armorer, AssignmentType.Goldsmith,
            AssignmentType.Leatherworker, AssignmentType.Weaver,
            AssignmentType.Alchemist, AssignmentType.Culinarian,

            AssignmentType.Miner, AssignmentType.Botanist,
            AssignmentType.Fisher,

            AssignmentType.Battlecraft,
            AssignmentType.TwinAdder,
            AssignmentType.ImmortalFlames,
            AssignmentType.Maelstorm,
        };

        public static void Draw()
        {

            using var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

            using (var child_NpcSelect = ImRaii.Child("Child: NPC Selection", new(200, ImGui.GetContentRegionAvail().Y), true))
            {
                if (!child_NpcSelect.Success)
                    return;

                using (var table = ImRaii.Table("Table: NPC Selection", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("Selected", ImGuiTableColumnFlags.WidthFixed, 10);
                    ImGui.TableSetupColumn("Name");

                    if (_validNpcs.Count() is 0)
                    {
                        _validNpcs = LeveInfo.Levemete_Info
                        .Where(x => x.Value.Leves.Count != 0)
                        .OrderBy(x => x.Value.TerritoryId)
                        .ToList();
                    }

                    foreach (var npc in _validNpcs)
                    {
                        bool isSelected = npc.Key == _selectedNpc;
                        float scale = ImGuiHelpers.GlobalScale;
                        float rowHeight = 25 * scale;

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (isSelected)
                        {
                            var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
                        }

                        ImGui.TableNextColumn();
                        ImGui.AlignTextToFramePadding();
                        var count = npc.Value.Leves.Count();

                        bool clicked = RowSelectable($"{npc.Value.Name} [{count:N0}]", isSelected, rowHeight);

                        if (clicked)
                        {
                            _selectedNpc = npc.Key;
                        }
                    }
                }
            }

            ImGui.SameLine();

            using (var child_NpcInfo = ImRaii.Child("Child: NPC Info", ImGui.GetContentRegionAvail(), true))
            {
                if (!child_NpcInfo.Success)
                    return;

                if (LeveInfo.Levemete_Info.TryGetValue(_selectedNpc, out var npcInfo))
                {
                    if (ImGui.Button($"Npc: {npcInfo.Name}"))
                    {
                        ImGui.SetClipboardText($"{_selectedNpc}, // {npcInfo.Name}");
                    }
                    var territory = ExcelHelper.Sheet_TerritoryType.GetRow(npcInfo.TerritoryId).PlaceName.Value.Name.ToString();
                    if (ImGui.Button($"Territory: {territory}"))
                    {
                        Utils.SetFlagForNPC(npcInfo.TerritoryId, npcInfo.Npc_Flag.X, npcInfo.Npc_Flag.Y);
                    }
                    Dictionary<AssignmentType, List<uint>> JobLeves = new();
                    foreach (var leve in npcInfo.Leves)
                    {
                        if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var leveInfo))
                        {
                            if (JobLeves.ContainsKey(leveInfo.JobAssignmentType))
                            {
                                JobLeves[leveInfo.JobAssignmentType].Add(leve);
                            }
                            else
                            {
                                JobLeves[leveInfo.JobAssignmentType] = new() { leve };
                            }
                        }
                    }

                    float iconSpacing = 6;
                    int itemsPerRow = 18;

                    var currentItem = 0;
                    var jobsToShow = _jobOrder
                        .Where(job => job == AssignmentType.None || JobLeves.ContainsKey(job))
                        .ToList();

                    foreach (var job in jobsToShow)
                    {
                        if (currentItem % itemsPerRow == 0)
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + iconSpacing);

                        JobToggleButton(job);
                        currentItem++;

                        if (currentItem % itemsPerRow != 0 && job != jobsToShow[^1])
                            ImGui.SameLine(0, iconSpacing);
                    }

                    using (var leveTable = ImRaii.Table("Leve Details: Table", 5, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.ScrollY, ImGui.GetContentRegionAvail()))
                    {
                        if (!leveTable.Success)
                            return;

                        ImGui.TableSetupColumn("Id");
                        ImGui.TableSetupColumn("Type");
                        ImGui.TableSetupColumn("Status");
                        ImGui.TableSetupColumn("Level");
                        ImGui.TableSetupColumn("Name");

                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableHeadersRow();

                        var jobsToDisplay = _selectedType == AssignmentType.None
                            ? _jobOrder.Where(j => j != AssignmentType.None)
                            : _jobOrder.Where(j => j == _selectedType);

                        foreach (var job in jobsToDisplay)
                        {
                            if (!JobLeves.TryGetValue(job, out var leves))
                                continue;

                            foreach (var leveId in leves.OrderBy(id => LeveInfo.Leve_SheetInfo[id].Level))
                            {
                                if (LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var sheetInfo))
                                {
                                    ImGui.TableNextRow();
                                    ImGui.TableSetColumnIndex(0);
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{leveId}");

                                    ImGui.TableNextColumn();
                                    GameIcons.DrawInline(LeveInfo.Assignment_IconDict[sheetInfo.JobAssignmentType].IconId);

                                    ImGui.TableNextColumn();
                                    uint statusId = Utils.Leve_IsComplete(leveId) ? LeveInfo.Leve_State[Leve_Status.NotComplete] : LeveInfo.Leve_State[Leve_Status.Complete];
                                    GameIcons.DrawInline(statusId, false);

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{sheetInfo.Level}");

                                    ImGui.TableNextColumn();
                                    ImGui.AlignTextToFramePadding();
                                    ImGui.Text($"{sheetInfo.LeveName}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    ImGui.Text($"No info exist on this npc: {_selectedNpc}");
                }
            }
        }

        public static bool RowSelectable(string id, bool isSelected, float height = 0f)
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
            bool enabled = _selectedType == selectedClass;

            using var framePadding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
            using var colors = ImRaii.PushColor(ImGuiCol.Button, Vector4.Zero); // Initialize with dummy
            using var styles = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 2.0f);

            if (enabled)
            {
                if (C.UseIceTheme)
                {
                    colors.Push(ImGuiCol.Button, Theme_Colors.LightSlate);
                    colors.Push(ImGuiCol.Border, Theme_Colors.IceBlue);
                }
                else
                {
                    colors.Push(ImGuiCol.Button, new Vector4(0.3f, 0.3f, 0.35f, 0.7f));
                    colors.Push(ImGuiCol.Border, ImGuiColors.ParsedGold);
                }
                styles.Push(ImGuiStyleVar.FrameBorderSize, 1.0f);
            }
            else if (C.UseIceTheme)
            {
                colors.Push(ImGuiCol.Button, Theme_Colors.DarkSlate);
                colors.Push(ImGuiCol.Border, Theme_Colors.TranslucentIce);
                styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
            }
            else
            {
                colors.Push(ImGuiCol.Button, new Vector4(0.2f, 0.2f, 0.2f, 0.1f));
                colors.Push(ImGuiCol.Border, new Vector4(0.4f, 0.4f, 0.4f, 0.5f));
                styles.Push(ImGuiStyleVar.FrameBorderSize, 0.5f);
            }

            var globalScale = ImGuiHelpers.GlobalScale;
            var size = new Vector2(28 * globalScale, 28 * globalScale);
            var iconId = LeveInfo.Assignment_IconDict[selectedClass].IconId;

            bool clicked = GameIcons.DrawButton(iconId, $"jobtoggle_{selectedClass}", size, grey: !enabled);

            if (clicked)
            {
                _selectedType = selectedClass;
            }
        }
    }
}
