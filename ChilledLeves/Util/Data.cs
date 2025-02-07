using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Util;

public static unsafe class Data
{
    // This is where the nightmare begins
    // Figuring how how the *-FUCKK-* to manage this in a neat and nicely manner

    // Some things to think about:
    // -> Godbert has virtually *-all-* the information in the "Leve" Sheet, which includes but not limited to
    // *THIS IS IN THE "Leve" Sheet
    // Key (leve id) (can be an int in this case... hopefully)
    // 0: Name (String)
    // 4: Assignment Type (Aka class it's tied to in the sheet) (int32)
    // 5: Town (ZoneID aka Int32)
    // 6: Level required for job (Uint16)
    // 8: Allowance Cost (Byte, usually 1 but this can change for grand scale FC stuff (think HW))
    // 9: Start Town {Int32)
    // 10: End Town (Int32)
    // 13: ClassJobCategory (Byte)
    // 17: Item Data ID (aka item being turned in) (Int32)
    // 
    // IF I START CARING TO PUT THIS IN AS WELL:
    // 21: Experience Reward (Uint32)
    // 22: Gil per leve (Uint32)

    // Now to get the actual information on what the leve entails more of, that's in "CraftLeve"
    // The key for this is from the sheet "Leve" on row #17 (DataId)
    // This value is given to give the sheet to tell
    // -> #2 How many times it can be repeated
    // -> # 3/4 | 5/6 | 7/8 | 9/10 are the item ID's + the amount that it needs to turnin

    // LeveAssignmentType is also a tab to keep note of
    // This has the icons for the leve types
    // 5 = CRP | 6 = BSM | 7 = ARM | 8 = GSM | 9 = LTW | 10 = WVR | 11 = ALC | 12 = CUL | BTN = 3 | MIN = 2 | FSH = 4

    // Baseline for the tables to be setup, this is moreso for organization of starting things (Can grab majority from the sheets but, need to have something to reference to begin with for Ui reasons I feel)
    // I can hear Croizat bitching at me for this already

    // List of all of the Crafter Job ID's into a nice checklist for self
    public static string SilverStarImage = "ChilledLeves.Assests.FavDisabled.png";
    public static string GoldStarImage = "ChilledLeves.Assests.FavEnabled.png";



    public static HashSet<uint> CrafterJobs = new() { 5, 6, 7, 8, 9, 10, 11, 12 };
    public static List<uint> VisibleLeves = new List<uint>();

    public static int JobSelected = 0;

    public class LeveDataforTable
    {
        public uint LeveID { get; set; }
        public int Amount { get; set; }

        public LeveDataforTable(uint leveID, int amount)
        {
            LeveID = leveID;
            Amount = amount;
        }
    }

    public class LeveDataDict
    {
        // Universal information across the leves
        /// <summary>
        /// Job ID Attached to the leve
        /// </summary>
        public uint JobID { get; set; }
        /// <summary>
        /// Level of the leve that you're undertaking, this is the *-minimum-* level you can be to do this leve
        /// </summary>
        public uint Level { get; set; }
        /// <summary>
        /// Name of the Leve
        /// </summary>
        public string LeveName { get; set; }
        /// <summary>
        /// Amount of times you want to do the leve. Good place to keep (in the dictionary... hopefully)
        /// </summary>
        public int Amount { get; set; }
        /// <summary>
        /// DataID (Row #17) aka QuestID of it. Leads to CraftLeves for crafters. Not sure about the rest though...
        /// </summary>
        public uint QuestID { get; set; }
        /// <summary>
        /// Starting city that the leve is in (good for specifics)
        /// </summary>
        public uint StartingCity { get; set; }
        /// <summary>
        /// Zone name that the leve starts in, (New Gridania, Upper Limsa... as examples)
        /// </summary>
        public string StartingZoneName { get; set; }
        /// <summary>
        /// EXP reward that you get while you're within that expansion's cap.
        /// </summary>
        public int ExpReward { get; set; }
        /// <summary>
        /// Gil Amount you can earn, this can be +- 5% of what is listed
        /// </summary>
        public int GilReward { get; set; }

        // Crafter Specific
        /// <summary>
        /// Crafter, ItemID for the turnin item
        /// </summary>
        public uint ItemID { get; set; }
        /// <summary>
        /// Crafter, Item Name itself for the Turnin
        /// </summary>
        public string ItemName { get; set; }
        /// <summary>
        /// Crafter, Item Icon itself, not really useful? Moreso visual flare for me
        /// </summary>
        public ISharedImmediateTexture? ItemIcon { get; set; }
        /// <summary>
        /// Crafter, amount of times you can do this turnin per leve. 
        /// Moreso for backend, might be able to show it somehow though...
        /// </summary>
        public int RepeatAmount { get; set; }
        /// <summary>
        /// Crafter, amount of items overall that is necessary for this leve for the full potentional
        /// </summary>
        public int TurninAmount { get; set; }
        /// <summary>
        /// Crafter, current amount of items that you have in your inventory. 
        /// Updates currently every second. 
        /// </summary>
        public int CurrentItemAmount { get; set; }
    }

    public class LeveType
    {
        /// <summary>
        /// Job icon that is grabbed from the sheet, just an easier way to pull it.
        /// </summary>
        public ISharedImmediateTexture? AssignmentIcon { get; set; }
        public IDalamudTextureWrap? FavoriteIcon { get; set; }
        /// <summary>
        /// Type of leve that is assigned to this. Quick and easy way to access this.
        /// </summary>
        public string LeveClassType { get; set; }
    }

    public class LeveEntry
    {
        public uint LeveID { get; set; }
        public int InputValue { get; set; }
    }

    public static Dictionary<uint, LeveDataDict> LeveDict = new();

    public static Dictionary<uint, LeveType> LeveTypeDict = new();
}
