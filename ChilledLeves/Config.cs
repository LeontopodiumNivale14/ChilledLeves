using System.Collections.Generic;
using ECommons.Configuration;

namespace ChilledLeves;

public class Config : IEzConfig
{
    public const int Version = 2;    
    public LeveFilterSettings LeveFilter { get; private set; } = new();
    public List<uint> FavoriteLeves = [];

    public class LeveFilterSettings
    {
        public bool OnlyFavorites = false;
        public bool ShowCompleted = true;
        public bool ShowIncomplete = true;
        public bool ShowBattleLeve = true;
        public bool ShowCarpenter = true;
        public bool ShowBlacksmith = true;
        public bool ShowArmorer = true;
        public bool ShowGoldsmith = true;
        public bool ShowLeatherworker = true;
        public bool ShowWeaver = true;
        public bool ShowAlchemist = true;
        public bool ShowCulinarian = true;
        public bool ShowMiner = true;
        public bool ShowBotanist = true;
        public bool ShowFisher = true;
        public bool ShowMaelstrom = true;
        public bool ShowTwinAdder = true;
        public bool ShowImmortalFlames = true;

        public int LevelFilter;
        public string NameFilter = "";
    }

    private List<uint> jobFilter = [];
    public List<LeveEntry> workList = new List<LeveEntry>();

    public void RefreshJobFilter()
    {
        jobFilter.Clear();
        GetJobFilter();
    }

    public List<uint> GetJobFilter()
    {
        if (jobFilter.Count == 0)
        {
            if (!C.LeveFilter.ShowMiner) jobFilter.Add(2);
            if (!C.LeveFilter.ShowBotanist) jobFilter.Add(3);
            if (!C.LeveFilter.ShowFisher) jobFilter.Add(4);

            if (!C.LeveFilter.ShowCarpenter) jobFilter.Add(5);
            if (!C.LeveFilter.ShowBlacksmith) jobFilter.Add(6);
            if (!C.LeveFilter.ShowArmorer) jobFilter.Add(7);
            if (!C.LeveFilter.ShowGoldsmith) jobFilter.Add(8);
            if (!C.LeveFilter.ShowLeatherworker) jobFilter.Add(9);
            if (!C.LeveFilter.ShowWeaver) jobFilter.Add(10);
            if (!C.LeveFilter.ShowAlchemist) jobFilter.Add(11);
            if (!C.LeveFilter.ShowCulinarian) jobFilter.Add(12);
        }

        return jobFilter;
    }

    public Dictionary<uint, bool> MountZone = new Dictionary<uint, bool>()
    {
        {135, true }, // Keep for now
        {137, true },
        {138, true }, 
        {140, true },
        {145, true },
        {146, true },
        {147, true },
        {148, true }, // Keep for now
        {152, true },
        {153, true },
        {155, true },
        {156, true }
    };

    public Dictionary<uint, bool> FlyZone = new Dictionary<uint, bool>()
    {
        {135, true },
        {137, true },
        {138, true },
        {140, true },
        {145, true },
        {146, true },
        {147, true },
        {148, true },
        {152, true },
        {153, true },
        {155, true },
        {156, true }
    };

    public void Save()
    {
        RefreshJobFilter();
        EzConfig.Save();
    }
}
