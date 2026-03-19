using ChilledLeves.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public Dictionary<LeveClass, bool> Job_LeveFilter { get; set; } = new()
    {
        [LeveClass.Crp] = true,
        [LeveClass.Bsm] = true,
        [LeveClass.Arm] = true,
        [LeveClass.Gsm] = true,
        [LeveClass.Ltw] = true,
        [LeveClass.Wvr] = true,
        [LeveClass.Alc] = true,
        [LeveClass.Cul] = true,
        [LeveClass.Min] = true,
        [LeveClass.Btn] = true,
        [LeveClass.Fsh] = true,
    };

    public Dictionary<string, bool> Leve_Filter { get; set; } = new()
    {
        ["Favorites"] = false,
        ["Completed"] = false,
        ["Incomplete"] = false,
    };

    public bool RapidImport { get; set; } = false;
    public int MinLevel { get; set; } = 0;
    public int MaxLevel { get; set; } = 100;
    public string NameSearch { get; set; } = "";
    public bool UseIceTheme { get; set; } = true;
}
