using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace ChilledLeves.Utilities;

internal static class ExcelHelper
{
    internal static ExcelSheet<TerritoryType>? Sheet_TerritoryType;
    internal static ExcelSheet<Leve>? Sheet_Leve;
    internal static ExcelSheet<Recipe> Sheet_Recipe;
    internal static ExcelSheet<Omen> Sheet_Omen;
    internal static ExcelSheet<Map> Sheet_Map;
    internal static ExcelSheet<LeveGuildleveAssignment> Sheet_leveText;

    public static void Init()
    {
        Svc.Data.GameData.Options.PanicOnSheetChecksumMismatch = false;
        Sheet_TerritoryType = Svc.Data.GetExcelSheet<TerritoryType>();
        Sheet_Leve = Svc.Data.GetExcelSheet<Leve>();
        Sheet_Recipe = Svc.Data.GetExcelSheet<Recipe>();
        Sheet_Omen = Svc.Data.GetExcelSheet<Omen>();
        Sheet_Map = Svc.Data.GetExcelSheet<Map>();
        Sheet_leveText = Svc.Data.Excel.GetSheet<LeveGuildleveAssignment>(name: "leve/GuildleveAssignment");
    }

    public static string GetTerritoryName(uint territoryid)
    {
        return Sheet_TerritoryType.GetRow(territoryid).PlaceName.Value.Name.ToString();
    }
}

[Sheet("leve/GuildleveAssignment")]
public readonly struct LeveGuildleveAssignment(ExcelPage page, uint offset, uint row) : IExcelRow<LeveGuildleveAssignment>
{
    public ExcelPage ExcelPage => page;
    public uint RowOffset => offset;
    public uint RowId => row;

    public readonly ReadOnlySeString Text0 => page.ReadString(offset, offset);
    public readonly ReadOnlySeString Text1 => page.ReadString(offset + 4, offset);

    static LeveGuildleveAssignment IExcelRow<LeveGuildleveAssignment>.Create(ExcelPage page, uint offset, uint row) =>
        new(page, offset, row);
}
