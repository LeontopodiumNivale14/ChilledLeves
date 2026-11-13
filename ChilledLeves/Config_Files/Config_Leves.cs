using ChilledLeves.Enums;
using ChilledLeves.Utilities.LeveData;
using System.Collections.Generic;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    // Playlist Leve Settings
    public Dictionary<uint, int> LeveList { get; set; } = new();
    public List<uint> LeveOrder { get; set; } = new();
    public bool GrabMulti { get; set; } = true;
    public List<uint> FavoriteLeves { get; set; } = new();
    public bool AllowMultiTurnin { get; set; } = true;
    public bool RepeatLastLeve { get; set; } = false;

    // ARR Leve Grind Settings
    public uint ARR_NpcId { get; set; } = 1000970;
    public AssignmentType ARR_SelectedAssignment { get; set; } = AssignmentType.Fisher;
    public StopConditions ARR_StopCondition { get; set; } = StopConditions.Level;
    public uint ARR_StopLevel { get; set; } = 100;
    public Dictionary<uint, List<uint>> ARR_LevemetPriority { get; set; } = new();
}
