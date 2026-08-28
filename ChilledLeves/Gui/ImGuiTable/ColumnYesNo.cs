using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Gui.ImGuiTable;

// Originally from HaselCommon
// https://github.com/Haselnussbomber/HaselCommon/blob/2b03c1d603931846e4f653893c65154c23f7ce36/HaselCommon/Gui/ImGuiTable/Table.cs

public class ColumnYesNo<TRow> : ColumnBool<TRow>
{
    public override string NameKeySpace => "ImGuiTable.ColumnYesNo";
    public override string[] Names => ["No", "Yes"];
}
