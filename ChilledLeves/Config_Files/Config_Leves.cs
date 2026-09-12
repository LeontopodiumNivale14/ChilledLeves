using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    // Playlist Leve Settings
    public Dictionary<uint, int> LeveList { get; set; } = new();
    public List<uint> LeveOrder { get; set; } = new();
    public bool GrabMulti { get; set; } = true;
    public List<uint> FavoriteLeves { get; set; } = new();
    public bool AllowMultiTurnin { get; set; } = true;
    public bool IncreaseDelay { get; set; } = false;
    public bool RepeatLastLeve { get; set; } = false;
    // TODO: Actually re-wire this in, and maybe actually create default best leveling plans with it
    public List<SavedList> Leve_Listing { get; set; } = new();

    // ARR Leve Grind Settings
    public uint ARR_NpcId { get; set; } = 1000970;
    public uint ARR_SelectedJob { get; set; } = 18;
    public Dictionary<uint, List<uint>> Npc_LevePriority { get; set; } = new();
    
    public class SavedList
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<(uint leve, int amount)> LeveList { get; set; } = new();
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public bool Locked { get; set; } = false;
    }
}
