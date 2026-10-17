using ChilledLeves.Enums;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public Dictionary<AssignmentType, bool> Assignemnt_Filter { get; set; } = new()
    {
        [AssignmentType.Carpenter] = true,
        [AssignmentType.Blacksmith] = true,
        [AssignmentType.Armorer] = true,
        [AssignmentType.Goldsmith] = true,
        [AssignmentType.Leatherworker] = true,
        [AssignmentType.Weaver] = true,
        [AssignmentType.Alchemist] = true,
        [AssignmentType.Culinarian] = true,
        [AssignmentType.Miner] = true,
        [AssignmentType.Botanist] = true,
        [AssignmentType.Fisher] = true,

        [AssignmentType.Battlecraft] = false,
        [AssignmentType.Maelstorm] = false,
        [AssignmentType.TwinAdder] = false,
        [AssignmentType.ImmortalFlames] = false,
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
    public bool UseIceTheme { get; set; } = true;
    public bool ShowActiveOverlay { get; set; } = true;
    public WindowSelection SelectedTab { get; set; } = WindowSelection.LeveInfo;
    public bool Compact_Ui { get; set; } = false;
}
