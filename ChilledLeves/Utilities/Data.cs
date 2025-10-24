using Dalamud.Game.ClientState.Statuses;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities;

public static unsafe class Data
{
    #region Leve Misc Information
    public static List<uint> VisibleLeves = new List<uint>();
    public static List<int> LeveStatus = new List<int>() { 71041, 71045, 71055 };

    public static HashSet<uint> SelectedLeves = new HashSet<uint>();

    public static List<LeveEntry> ListCycled = new List<LeveEntry>();
    public static List<LeveEntry> TemporaryList = new List<LeveEntry>();

    #endregion

    #region Aethernet System

    public class AethernetSystem
    {
        public required uint AetheryteID { get; set; }
        public required uint Aethernet { get; set; }
        public required Vector3 CrystalLoc { get; set; }
        public required Vector3 TPLocation { get; set; }
        public required uint TeleportZone { get; set; }
        public required float Range { get; set; }
    }

    public static Dictionary<uint, AethernetSystem> AethernetDict = new Dictionary<uint, AethernetSystem>()
    {
        // Limsa Lower to Upper
        { 128, new AethernetSystem { AetheryteID = 8, Aethernet = 1, CrystalLoc = new Vector3(-84.03f, 20.77f, 0.02f), TPLocation = new Vector3 (15.95f, 40.00f, 71.38f), TeleportZone = 128, Range = 7} },
        // Limsa Upper to Lower
        { 129, new AethernetSystem { AetheryteID = 41, Aethernet = 0, CrystalLoc = new Vector3(16.48f, 40.00f, 71.23f), TPLocation = new Vector3(-84.03f, 20.77f, 0.02f), TeleportZone = 129, Range = 15} },
        // Ul'dah Main -> Adventurer's Guild
        { 130, new AethernetSystem { AetheryteID = 9, Aethernet = 1, CrystalLoc = new Vector3(-144.52f, -1.36f, -169.67f), TPLocation = new Vector3 (65.32f, 4.00f, -117.29f), TeleportZone = 130, Range = 5} },
        // Crystarium -> Upper floor
        { 819, new AethernetSystem { AetheryteID = 133, Aethernet = 6, CrystalLoc = new Vector3(-65.02f, 4.53f, 0.02f), TPLocation = new Vector3 (-50.71f, 20.00f, -170.41f), TeleportZone = 819, Range = 5} }
    };

    #endregion

    #region Npc Information

    public class NpcData
    {
        public string NpcName { get; set; }
        public string NpcLocation { get; set; }
        public string NpcLevels { get; set; }
    }

    public static Dictionary<uint, NpcData> NpcLocationInfo = new Dictionary<uint, NpcData>();

    #endregion

    #region NPC Leve Information

    #endregion

    public static Dictionary<uint, ISharedImmediateTexture> GreyTexture = new Dictionary<uint, ISharedImmediateTexture>();

    public static Dictionary<uint, ISharedImmediateTexture> LeveStatusDict = new Dictionary<uint, ISharedImmediateTexture>();

    // Used in the config

    #region Config classes

    public class LeveEntry
    {
        public uint LeveID { get; set; }
        public int InputValue { get; set; }
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

        public int LevelFilter;
        public string NameFilter = "";
    }

    #endregion

    #region Gathering

    #region G.Actions

    public class GatheringActions
    {
        public string ActionName { get; set; }
        public string InternalName { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }

    public static Dictionary<string, GatheringActions> BtnActionDict = new()
    {
        { "BoonIncrease1", new GatheringActions
        {
            ActionName = "Pioneer's Gift I",
            InternalName = "",
            StatusId = 2666,
            StatusName = "Gift of the Land"
        }},
        { "BoonIncrease2", new GatheringActions
        {
            ActionName = "Pioneer's Gift II",
            InternalName = "",
            StatusId = 759,
            StatusName = "Gift of the Land II"
        }},
        { "Tidings", new GatheringActions
        {
            ActionName = "Nophica's Tidings",
            InternalName = "",
            StatusId = 2667,
            StatusName = "Gatherer's Bounty"
        }},
        { "YieldI", new GatheringActions
        {
            ActionName = "Blessed Harvest",
            InternalName = "",
            StatusId = 219,
            StatusName = "Gathering Yield Up"
        }},
        { "YieldII", new GatheringActions
        {
            ActionName = "Blessed Harvest II",
            InternalName = "",
            StatusId = 219,
            StatusName = "Gathering Yield Up"
        }},
        { "215", new GatheringActions
        {
            ActionName = "Ageless Words",
            InternalName = "",
        }},
        { "26522", new GatheringActions
        {
            ActionName = "",
            InternalName = "",
            StatusId = 2765,
            StatusName = ""
        }},
    };


    #endregion

    #endregion
}
