using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace ChilledLeves.Utilities;

internal static class ExcelHelper
{
    internal static ExcelSheet<TerritoryType> Sheet_TerritoryType;
    internal static ExcelSheet<Leve> Sheet_Leve;
    internal static ExcelSheet<Recipe> Sheet_Recipe;
    internal static ExcelSheet<Omen> Sheet_Omen;
    internal static ExcelSheet<Map> Sheet_Map;
    internal static ExcelSheet<LeveGuildleveAssignment> Sheet_leveText;
    internal static ExcelSheet<Aetheryte> Sheet_Aetheryte;

    internal static ExcelSheet<Item> Sheet_Item;
    internal static ExcelSheet<EventItem> Sheet_EventItem;

    internal static ExcelSheet<Lumina.Excel.Sheets.Action> Sheet_Action;
    internal static ExcelSheet<Level> Sheet_Level;
    internal static ExcelSheet<GatheringExp> Sheet_GatheringExp;
    internal static ExcelSheet<Mount> Sheet_Mount;

    public static void Init()
    {
        Svc.Data.GameData.Options.PanicOnSheetChecksumMismatch = false;
        Sheet_TerritoryType = Svc.Data.GetExcelSheet<TerritoryType>();
        Sheet_Leve = Svc.Data.GetExcelSheet<Leve>();
        Sheet_Recipe = Svc.Data.GetExcelSheet<Recipe>();
        Sheet_Omen = Svc.Data.GetExcelSheet<Omen>();
        Sheet_Map = Svc.Data.GetExcelSheet<Map>();
        Sheet_leveText = Svc.Data.Excel.GetSheet<LeveGuildleveAssignment>(name: "leve/GuildleveAssignment");
        Sheet_Aetheryte = Svc.Data.GetExcelSheet<Aetheryte>();

        Sheet_Item = Svc.Data.GetExcelSheet<Item>();
        Sheet_EventItem = Svc.Data.GetExcelSheet<EventItem>();

        Sheet_Action = Svc.Data.GetExcelSheet<Lumina.Excel.Sheets.Action>();
        Sheet_Level = Svc.Data.GetExcelSheet<Level>();
        Sheet_GatheringExp = Svc.Data.GetExcelSheet<GatheringExp>();
        Sheet_Mount = Svc.Data.GetExcelSheet<Mount>();
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
