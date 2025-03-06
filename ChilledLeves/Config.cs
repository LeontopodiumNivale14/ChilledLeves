using System.Collections.Generic;
using System.Text.Json.Serialization;
using ECommons.Configuration;
using Lumina.Excel.Sheets;

namespace ChilledLeves;

public class Config : IEzConfig
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 4;

    public uint CompleteFilter { get; set; } = 0;

    private List<uint> jobFilter = [];
    public List<LeveEntry> workList = new List<LeveEntry>();

    public List<uint> GetJobFilter()
    {
        if (jobFilter.Count == 0)
        {
            if (!ShowMiner) jobFilter.Add(2);
            if (!ShowBotanist) jobFilter.Add(3);
            if (!ShowFisher) jobFilter.Add(4);

            if (!ShowCarpenter) jobFilter.Add(5);
            if (!ShowBlacksmith) jobFilter.Add(6);
            if (!ShowArmorer) jobFilter.Add(7);
            if (!ShowGoldsmith) jobFilter.Add(8);
            if (!ShowLeatherworker) jobFilter.Add(9);
            if (!ShowWeaver) jobFilter.Add(10);
            if (!ShowAlchemist) jobFilter.Add(11);
            if (!ShowCulinarian) jobFilter.Add(12);
        }

        return jobFilter;
    }

    public bool OnlyFavorites = false;
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
    public bool GrabMulti = true;

    public List<uint> FavoriteLeves = [];

    public bool IncreaseDelay = false;

    public void RefreshJobFilter()
    {
        jobFilter.Clear();
        GetJobFilter();
    }

    public void Save()
    {
        RefreshJobFilter();
        EzConfig.Save();
    }

}
