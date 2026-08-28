using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using System.Collections.Generic;

namespace ChilledLeves.Gui.ImGuiTable;

// Originally from Dalamud
// https://github.com/goatcorp/Dalamud/tree/master/Dalamud/Interface/Utility/Table

// And also from HaselCommon:
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/Table.cs

public static class Table
{
    public const float ArrowWidth = 10;
}

public partial class Table<T> : IDisposable
{
    protected readonly TextService _textService;
    protected List<T> _rows = [];
    protected List<T>? _filteredRows;
    protected bool _closePopupNextFrame;

    public string? Id { get; set; }
    public List<T> Rows
    {
        get => _rows;
        set { _rows = value; RowsLoaded = true; IsSortDirty |= true; }
    }
    public List<T>? FilteredRows => _filteredRows;
    public bool RowsLoaded { get; set; }
    public List<Column<T>> Columns { get; set; } = [];
    public bool IsFilterDirty { get; set; } = true;
    public bool IsSortDirty { get; set; } = true;
    public int ScrollFreezeCols { get; set; } = 1;
    public bool Sortable
    {
        get => Flags.HasFlag(ImGuiTableFlags.Sortable);
        set => Flags = value ? Flags | ImGuiTableFlags.Sortable : Flags & ~ImGuiTableFlags.Sortable;
    }
    public bool ClosePopup { get; set; }

    public ImGuiTableFlags Flags = ImGuiTableFlags.RowBg
      | ImGuiTableFlags.Sortable
      | ImGuiTableFlags.BordersOuter
      | ImGuiTableFlags.ScrollY
      | ImGuiTableFlags.BordersInnerV
      | ImGuiTableFlags.NoBordersInBodyUntilResize;

    private void Initialize()
    {

    }

    public virtual void Dispose()
    {

    }

    public virtual void OnLanguageChanged(string langCode)
    {
        if (RowsLoaded)
        {
            Rows.Clear();
            LoadRows();

            foreach (var column in Columns)
            {
                column.OnLanguageChanged(langCode);
                UpdateColumnLabel(column);
            }
        }

        IsSortDirty |= true;
    }

    public void Draw()
    {
        using var id = ImRaii.PushId(Id!, !string.IsNullOrEmpty(Id));
        DrawTableInternal();
    }

    public virtual float CalculateLineHeight()
    {
        return ImGui.GetTextLineHeightWithSpacing();
    }

    public virtual void LoadRows()
    {
    }

    private void SortInternal()
    {
        if (!Sortable)
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        IsSortDirty |= sortSpecs.SpecsDirty;

        if (!IsSortDirty)
            return;

        if (sortSpecs.SpecsCount == 0)
        {
            if (Flags.HasFlag(ImGuiTableFlags.SortTristate))
                SortTristate();
        }
        else if (Columns.Count > 0)
        {
            var column = Columns[Columns.Count <= sortSpecs.Specs.ColumnIndex ? 0 : sortSpecs.Specs.ColumnIndex];
            _rows.Sort((a, b) => (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending ? -1 : 1) * column.Compare(a, b));
        }

        _filteredRows = null;
        sortSpecs.SpecsDirty = IsSortDirty = false;
    }

    public virtual void SortTristate()
    {
        throw new NotImplementedException();
    }

    private void DrawTableInternal()
    {
        if (Columns.Count == 0)
            return;

        using var table = ImRaii.Table("Table", Columns.Count, Flags, ImGui.GetContentRegionAvail());
        if (!table)
            return;

        ImGui.TableSetupScrollFreeze(ScrollFreezeCols, 1);

        foreach (var column in Columns)
            ImGui.TableSetupColumn(column.Label, column.Flags, column.Width * ImGuiHelpers.GlobalScale);

        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        if (_closePopupNextFrame)
        {
            ImGuiP.ClosePopupsExceptModals();
            _closePopupNextFrame = false;
        }

        if (ClosePopup && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
        {
            ClosePopup = false;
            _closePopupNextFrame = true;
        }

        foreach (var (columnIndex, column) in Columns.Index())
        {
            if (!ImGui.TableSetColumnIndex(columnIndex))
                continue;

            using var id = ImRaii.PushId($"ColumnHeader{columnIndex}");

            using (ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero))
            {
                ImGui.TableHeader(string.Empty);
                ImGui.SameLine();
            }

            if (column.AutoLabel && string.IsNullOrEmpty(column.Label))
                UpdateColumnLabel(column);

            IsFilterDirty |= column.DrawFilter();
        }

        if (!RowsLoaded)
        {
            LoadRows();
            RowsLoaded = true;
        }

        SortInternal();

        if (_filteredRows == null || IsFilterDirty)
        {
            _filteredRows?.Clear();
            _filteredRows ??= [];
            _filteredRows.AddRange(_rows.Where(row => Columns.All(column => column.ShouldShow(row))));

            IsFilterDirty = false;
        }

        var lineHeight = CalculateLineHeight();
        if (lineHeight == 0)
        {
            foreach (var (index, row) in _filteredRows.Index())
                DrawRow(row, index);
        }
        else
        {
            ImGuiClip.ClippedDraw(_filteredRows, DrawRow, lineHeight);
        }
    }

    private void DrawRow(T row, int rowIndex)
    {
        var column = 0;
        using var id = ImRaii.PushId(rowIndex);
        foreach (var header in Columns)
        {
            id.Push(column++);
            if (ImGui.TableNextColumn())
                header.DrawColumn(row);
            id.Pop();
        }
    }

    private void UpdateColumnLabel<TColumn>(Column<TColumn> column)
    {
        if (column.AutoLabel)
            column.Label = column.LabelKey;
    }
}
