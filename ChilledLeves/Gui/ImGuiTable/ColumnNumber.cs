using Dalamud.Interface.Utility.Table;

namespace ChilledLeves.Gui.ImGuiTable;

// Originally from HaselCommon
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/ColumnNumber.cs

public class ColumnNumber<T> : ColumnString<T>
{
    public enum ComparisonMethod : byte
    {
        Equal = 0,
        LessEqual = 1,
        GreaterEqual = 2,
    };

    protected ComparisonMethod Comparison = ComparisonMethod.Equal;
    protected int? FilterNumber;

    public virtual int ToValue(T row)
        => row!.GetHashCode();

    public override int Compare(T lhs, T rhs)
        => ToValue(lhs).CompareTo(ToValue(rhs));

    public override string ToName(T row)
        => ToValue(row).ToString();

    public override bool DrawFilter()
    {
        if (!base.DrawFilter())
            return false;

        if (int.TryParse(FilterValue, out var number))
        {
            FilterNumber = number;
            FilterRegex = null;
        }
        else
        {
            FilterNumber = null;
        }

        return true;
    }

    public override bool ShouldShow(T row)
    {
        if (!FilterNumber.HasValue)
            return base.ShouldShow(row);

        var value = ToValue(row);
        return Comparison switch
        {
            ComparisonMethod.Equal => value == FilterNumber.Value,
            ComparisonMethod.LessEqual => value <= FilterNumber.Value,
            ComparisonMethod.GreaterEqual => value >= FilterNumber.Value,
            _ => true,
        };
    }
}
