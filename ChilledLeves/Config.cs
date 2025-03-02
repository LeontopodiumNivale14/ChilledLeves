using System.Collections.Generic;
using System.Text.Json.Serialization;
using ECommons.Configuration;
using Lumina.Excel.Sheets;

namespace ChilledLeves;

public class Config : IEzConfig
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 3;

    private List<uint> jobFilter = [];
    public List<LeveEntry> workList = new List<LeveEntry>();

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

    public LeveFilterSettings LeveFilter { get; private set; } = new();
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
