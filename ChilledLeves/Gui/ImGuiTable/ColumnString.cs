using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility.Table;
using System.Text.RegularExpressions;

namespace ChilledLeves.Gui.ImGuiTable;

// Originally from HaselCommon
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/ColumnString.cs

public class ColumnString<T> : Column<T>
{
    public ColumnString()
    {
        Flags &= ~ImGuiTableColumnFlags.NoResize;
    }

    protected Regex? FilterRegex;

    public string FilterValue { get; set; } = string.Empty;

    public virtual string ToName(T row)
        => row!.ToString() ?? string.Empty;

    public override int Compare(T lhs, T rhs)
        => string.Compare(ToName(lhs), ToName(rhs), StringComparison.InvariantCulture);

    public override bool DrawFilter()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);

        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var tmp = FilterValue;
        if (!ImGui.InputTextWithHint("##Filter", Label, ref tmp, 256, ImGuiInputTextFlags.AutoSelectAll) || tmp == FilterValue)
            return false;

        FilterValue = tmp;
        try
        {
            FilterRegex = new Regex(FilterValue, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch
        {
            FilterRegex = null;
        }

        return true;
    }

    public override bool ShouldShow(T row)
    {
        var name = ToName(row);
        if (FilterValue.Length == 0)
            return true;

        return FilterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.OrdinalIgnoreCase);
    }

    public override void DrawColumn(T row)
    {
        ImGui.Text(ToName(row));
    }
}