using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public Dictionary<string, bool> Job_Filter { get; set; } = new()
    {
        ["Carpenter"] = true,
        ["Blacksmith"] = true,
        ["Armorer"] = true,
        ["Goldsmith"] = true,
        ["Leatherworker"] = true,
        ["Weaver"] = true,
        ["Alchemist"] = true,
        ["Culinarian"] = true,
        ["Miner"] = true,
        ["Botanist"] = true,
        ["Fisher"] = true,
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
