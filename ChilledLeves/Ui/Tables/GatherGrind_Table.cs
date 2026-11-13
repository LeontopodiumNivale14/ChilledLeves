using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Gui.ImGuiTable;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using ECommons.ExcelServices;
using System.Collections.Generic;

namespace ChilledLeves.Ui.Tables;

internal class GatherGrind_Table
{
    public class VendorInfo
    {
        public VendorInfo(uint npcId, LeveInfo.Info_Vendor info)
        {
            NpcId = npcId;
            Name = info.Name;
            TerritoryId = info.TerritoryId;
            TerritoryName = ExcelHelper.Sheet_TerritoryType.GetRow(info.TerritoryId).PlaceName.Value.Name.ToString();

            LevesByAssignment = info.Leves
                .GroupBy(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType)
                .ToDictionary(g => g.Key, g => g.ToList());

            LevelRanges = LevesByAssignment.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    var levels = kvp.Value.Select(x => LeveInfo.Leve_SheetInfo[x].Level).ToList();
                    return (Min: levels.Min(), Max: levels.Max());
                });
        }

        public uint NpcId { get; }
        public string Name { get; }
        public uint TerritoryId { get; }
        public string TerritoryName { get; }
        public Dictionary<AssignmentType, List<uint>> LevesByAssignment { get; }

        // Lowest and highest leve level per job. No entry means no leves for that job.
        public Dictionary<AssignmentType, (uint Min, uint Max)> LevelRanges { get; }

        public int LeveCount(AssignmentType assignment)
            => LevesByAssignment.TryGetValue(assignment, out var leves) ? leves.Count : 0;

        public IEnumerable<uint> GetLevels(AssignmentType assignment)
        {
            if (!LevesByAssignment.TryGetValue(assignment, out var leves))
                return [];

            return leves
                .Select(x => LeveInfo.Leve_SheetInfo[x].Level)
                .Distinct()
                .OrderBy(x => x);
        }

        public bool AllCompleted(AssignmentType assignment)
        {
            if (!LevesByAssignment.TryGetValue(assignment, out var leves))
                return false;

            return leves.All(x => Utils.Leve_IsComplete(x));
        }
    }

    public class VendorTable : Table<VendorInfo>
    {
        private const float TableWidth = 650;
        private const float TableHeight = 350;

        // Unscaled. Try 1 first, then 0 if you want it really tight.
        private const float CellPaddingY = 1;

        public VendorTable(IReadOnlyList<AssignmentType> validAssignments, Action<AssignmentType, Vector2> drawIcon)
        {
            ArgumentNullException.ThrowIfNull(validAssignments);
            ArgumentNullException.ThrowIfNull(drawIcon);

            Id = "Gathering: Vendor Selection";
            ScrollFreezeCols = 0;

            // Flags must be assigned BEFORE Sortable, because Sortable edits Flags.
            Flags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY;
            Sortable = true;

            var columns = new List<Column<VendorInfo>>
        {
            new SelectedColumn(),
            new NpcColumn(),
            new TerritoryColumn(),
        };

            foreach (var assignment in validAssignments)
                columns.Add(new JobColumn(assignment, drawIcon));

            Columns = columns;
        }

        protected override Vector2 GetOuterSize()
            => new Vector2(TableWidth, TableHeight) * ImGuiHelpers.GlobalScale;

        // Only tighten the rows for the Ice theme; otherwise keep the theme's padding.
        protected override Vector2? GetCellPadding()
        {
            if (!C.UseIceTheme)
                return null;

            return new Vector2(ImGui.GetStyle().CellPadding.X, CellPaddingY * ImGuiHelpers.GlobalScale);
        }

        // 0 = draw every row, no clipping. Rows can vary in height (icons, theme padding),
        // and the clipper assumes a fixed height, which hides rows at the bottom.
        public override float CalculateLineHeight()
            => 0;

        public override void LoadRows()
        {
            var rows = new List<VendorInfo>();
            foreach (var npc in LeveInfo.GatherGrind_Npcs)
            {
                if (!LeveInfo.Levemete_Info.TryGetValue(npc, out var info))
                    continue;

                rows.Add(new VendorInfo(npc, info));
            }

            Rows = rows;
        }
    }

    public sealed class SelectedColumn : Column<VendorInfo>
    {
        public SelectedColumn()
        {
            Label = "##Selected";
            SetFixedWidth(30); // Table.cs already multiplies by GlobalScale
        }

        private static bool IsSelected(VendorInfo row)
            => C.ARR_NpcId == row.NpcId;

        // Base DrawFilter would print the literal "##Selected" text.
        public override bool DrawFilter()
            => false;

        // Selected rows first when sorting ascending.
        public override int Compare(VendorInfo lhs, VendorInfo rhs)
            => IsSelected(rhs).CompareTo(IsSelected(lhs));

        public override void DrawColumn(VendorInfo row)
        {
            if (!IsSelected(row))
                return;

            var color = C.UseIceTheme ? Theme_Colors.SidebarAccent : ImGuiColors.ParsedGold;
            ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, ImGui.GetColorU32(color));
        }
    }

    public sealed class NpcColumn : ColumnString<VendorInfo>
    {
        public NpcColumn()
        {
            Label = "Npc";

            // Fixed width of 0 tells ImGui to auto-fit the column to its content,
            // so Location gets whatever space is left over.
            SetFixedWidth(0);
        }

        public override string ToName(VendorInfo row)
            => row.Name;

        public override void DrawColumn(VendorInfo row)
        {
            var clicked = RowSelectable(row.Name, C.ARR_NpcId == row.NpcId);
            if (!clicked)
                return;

            C.ARR_NpcId = row.NpcId;
            C.Save();
            ImGui.CloseCurrentPopup();
        }
    }

    public sealed class TerritoryColumn : ColumnString<VendorInfo>
    {
        public TerritoryColumn()
        {
            Label = "Location";
        }

        public override string ToName(VendorInfo row)
            => row.TerritoryName;
    }

    public sealed class JobColumn : Column<VendorInfo>
    {
        private readonly AssignmentType _assignment;
        private readonly Action<AssignmentType, Vector2> _drawIcon;

        public JobColumn(AssignmentType assignment, Action<AssignmentType, Vector2> drawIcon)
        {
            _assignment = assignment;
            _drawIcon = drawIcon;

            // Enum name rather than LeveInfo.Assignment_IconDict, so this is safe to
            // construct during static initialization.
            Label = assignment.ToString();
            SetFixedWidth(60);
        }

        // Ascending order:
        //   1. Vendors whose lowest leve is lowest come first
        //   2. Ties are broken by the highest leve (lower first)
        //   3. Vendors with no leves for this job come last
        // Returning a positive number means "lhs goes AFTER rhs".
        public override int Compare(VendorInfo lhs, VendorInfo rhs)
        {
            var hasLhs = lhs.LevelRanges.TryGetValue(_assignment, out var lhsRange);
            var hasRhs = rhs.LevelRanges.TryGetValue(_assignment, out var rhsRange);

            if (!hasLhs && !hasRhs)
                return 0;

            if (!hasLhs)
                return 1; // lhs has none, so it goes after rhs

            if (!hasRhs)
                return -1; // rhs has none, so lhs goes before it

            var minCompare = lhsRange.Min.CompareTo(rhsRange.Min);
            if (minCompare != 0)
                return minCompare;

            return lhsRange.Max.CompareTo(rhsRange.Max);
        }

        public override void DrawColumn(VendorInfo row)
        {
            if (row.LeveCount(_assignment) == 0)
                return;

            _drawIcon(_assignment, new Vector2(ImGui.GetTextLineHeight()));

            if (ImGui.IsItemHovered())
                ImGui.SetTooltip($"Leve Levels: {string.Join(", ", row.GetLevels(_assignment))}");

            if (!row.AllCompleted(_assignment))
                return;

            ImGui.SameLine();
            GameIcons.DrawInlineOrIcon(071045, FontAwesomeIcon.Check);
        }
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
}