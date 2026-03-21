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
    public bool AllowMultiTurnin { get; set; } = true;
    public Dictionary<uint, int> LeveList { get; set; } = new();
    public List<uint> LeveOrder { get; set; } = new();
    public Dictionary<uint, List<uint>> Npc_LevePriority { get; set; } = new();
    public List<SavedList> Leve_Listing { get; set; } = new();
    
    public class SavedList
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<(uint leve, int amount)> LeveList { get; set; } = new();
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
