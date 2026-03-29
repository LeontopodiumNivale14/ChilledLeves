using ChilledLeves.Enums;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public Dictionary<Job, bool> Job_LeveFilter { get; set; } = new()
    {
        [Job.CRP] = true,
        [Job.BSM] = true,
        [Job.ARM] = true,
        [Job.GSM] = true,
        [Job.LTW] = true,
        [Job.WVR] = true,
        [Job.ALC] = true,
        [Job.CUL] = true,
        [Job.MIN] = true,
        [Job.BTN] = true,
        [Job.FSH] = true,
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
