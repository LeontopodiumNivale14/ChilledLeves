using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility.Table;
using System.Collections.Generic;

namespace ChilledLeves.Gui.ImGuiTable;

// Originally from HaselCommon
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/ColumnFlags.cs

public class ColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public T AllFlags = default;

    public virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    public virtual string NameKeySpace => "";

    public virtual string[] Names
        => Enum.GetNames<T>();

    public virtual T FilterValue
        => default;

    public virtual void SetValue(T value, bool enable)
        => throw new NotImplementedException();

    public override bool DrawFilter()
    {
        using var id = ImRaii.PushId("##Filter");
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var all = FilterValue.HasFlag(AllFlags);
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImRaii.Combo(string.Empty, Label, ImGuiComboFlags.NoArrowButton);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SetValue(AllFlags, true);
            return true;
        }

        if (!all && ImGui.IsItemHovered())
            ImGui.SetTooltip("Right-click to clear filters.");

        if (!combo)
            return false;

        color.Pop();

        var ret = false;
        if (ImGui.Checkbox("Enable All", ref all))
        {
            SetValue(AllFlags, all);
            ret = true;
        }

        using var indent = ImRaii.PushIndent(10f);
        for (var i = 0; i < Names.Length; ++i)
        {
            var tmp = FilterValue.HasFlag(Values[i]);
            if (!ImGui.Checkbox($"{Names[i]}", ref tmp))
                continue;

            SetValue(Values[i], tmp);
            ret = true;
        }

        return ret;
    }
}

