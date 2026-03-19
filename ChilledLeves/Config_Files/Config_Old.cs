using ChilledLeves.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public const int CurrentConfigVersion = 6;
    public uint CompleteFilter { get; set; } = 0;
    public List<LeveEntry> workList = new List<LeveEntry>();

    public int LevelFilter;
    public string NameFilter = "";
    public string SelectedNpcName = "Gontrant";
    public string LocationName = "New Gridania";
    public string ClassSelected = "Fisher";
    public uint ClassJobType = 4;
    public int LevelSliderInput = 1;
    public Dictionary<uint, int> LevePriority = new Dictionary<uint, int>();
}
