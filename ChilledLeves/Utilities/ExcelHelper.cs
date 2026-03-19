using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities;

internal static class ExcelHelper
{
    internal static ExcelSheet<TerritoryType>? Sheet_TerritoryType;
    internal static ExcelSheet<Leve>? Sheet_Leve;
    internal static ExcelSheet<Recipe> Sheet_Recipe;

    public static void Init()
    {
        Svc.Data.GameData.Options.PanicOnSheetChecksumMismatch = false;
        Sheet_TerritoryType = Svc.Data.GetExcelSheet<TerritoryType>();
        Sheet_Leve = Svc.Data.GetExcelSheet<Leve>();
        Sheet_Recipe = Svc.Data.GetExcelSheet<Recipe>();
    }
}
