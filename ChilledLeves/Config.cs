using System.Text.Json.Serialization;
using ECommons.Configuration;

namespace ChilledLeves;

public class Config : IEzConfig
{
    [JsonIgnore]
    public const int CurrentConfigVersion = 1;

    public LeveFilterSettings LeveFilter { get; private set; } = new();

    public void Save()
    {
        EzConfig.Save();
    }

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
    }
}
