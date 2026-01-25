using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public bool GrabMulti { get; set; } = true;
    public uint SelectedNpcId { get; set; } = 1000970;
    public uint Priority_SelectedJob { get; set; } = 18;
    public List<uint> FavoriteLeves { get; set; } = new();
    public bool IncreaseDelay { get; set; } = false;
    public bool RepeatLastLeve { get; set; } = false;
    public Dictionary<uint, int> LeveList { get; set; } = new();
    public Dictionary<uint, List<uint>> Npc_LevePriority { get; set; } = new();
}
