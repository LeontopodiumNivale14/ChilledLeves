using Dalamud.Interface.Utility.Table;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace ChilledLeves.Gui.ImGuiTable;

// Origionally From HaselCommon
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/ColumnBool.cs

[Flags]
public enum BoolValues
{
    False = 1,
    True = 2,
}

public class ColumnBool<TRow> : ColumnFlags<BoolValues, TRow>
{
    private BoolValues _filterValue;
    public override BoolValues FilterValue => _filterValue;

    public ColumnBool()
    {
        AllFlags = Enum.GetValues<BoolValues>().Aggregate((a, b) => a | b);
        _filterValue = AllFlags;
    }

    public override string NameKeySpace => "ImGuiTable.ColumnBool";

    public virtual bool ToBool(TRow row)
        => true;

    public override bool ShouldShow(TRow row)
    {
        var value = ToBool(row);
        return (FilterValue.HasFlag(BoolValues.True) && value) ||
               (FilterValue.HasFlag(BoolValues.False) && !value);
    }

    public override void DrawColumn(TRow row)
    {
        var value = ToBool(row);
        var color = value ? EColor.Green : EColor.Red;
        string text = value ? "1" : "0";

        ImGuiEx.Text(color, text);
    }

    public override int Compare(TRow a, TRow b)
        => ToBool(a).CompareTo(ToBool(b));

    public override void SetValue(BoolValues value, bool enable)
    {
        if (enable)
            _filterValue |= value;
        else
            _filterValue &= ~value;
    }
}

