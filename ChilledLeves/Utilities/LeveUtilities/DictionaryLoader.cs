using ChilledLeves.Utilities.UtilityClass;
using Dalamud.Interface.Textures;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentMiragePrismPrismSetConvert.AgentData;

namespace ChilledLeves.Utilities.LeveUtilities
{
    public static partial class LeveInfo
    {
        public class CrafterData
        {
            /// <summary>
            /// The person who you're turning the leve into. 
            /// This is also not hardcoded into the sheets anywhere... man this is rough lolol
            /// </summary>
            public uint LeveTurninVendorID { get; set; }

            // Crafter/Fisher Specific
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
        }
        public class LeveData
        {
            // Universal information across the leves
            /// <summary>
            /// #0 [Name] Name of the Leve
            /// </summary>
            public required string LeveName { get; set; }
            /// <summary>
            /// The value that the Ecom job is assigned to. Used for gear switching/class switching.
            /// </summary>
            public uint JobId { get; set; }
            /// <summary>
            /// #6 [Class Job Level] Level of the leve that you're undertaking, this is the *-minimum-* level you can be to do this leve
            /// </summary>
            public uint Level { get; set; }
            /// <summary>
            /// The person who is distributing this leve <br></br>
            /// This is... the most painful part becuase there's NOWHERE in the sheets that references this information at all. AT ALL. <br></br>
            /// Original solution was the best solution it runs out... :/
            /// </summary>
            public uint LeveVendorID { get; set; }
            public uint TurninVendorId { get; set; }
            /// <summary>
            /// #17 [DataID] aka QuestID of it. Leads to CraftLeves for crafters. Not sure about the rest though...
            /// </summary>
            public uint QuestID { get; set; }
            /// <summary>
            /// #21 [EXP Reward] EXP that you get while you're within that expansion's cap.
            /// </summary>
            public int ExpReward { get; set; }
            /// <summary>
            /// #22 [Gil Reward] Gil Amount you can earn, this can be +- 5% of what is listed
            /// </summary>
            public int GilReward { get; set; }
            /// <summary>
            /// How many leves are required for this leve?
            /// </summary>
            public int AllowanceCost { get; set; }
            public CrafterData CrafterInfo { get; set; }
        }
        public static Dictionary<uint, LeveData> LeveDictionary = new();

        public static void PopulateDictionary()
        {
            // General overview of all the leves that exist in the game
            var LeveSheet = Svc.Data.GetExcelSheet<Leve>();

            // This contains all the crafter leves + the fisher leves
            var CraftLeveSheet = Svc.Data.GetExcelSheet<CraftLeve>();

            // Item Sheet that contains... well all the items
            var ItemSheet = Svc.Data.GetExcelSheet<Item>();

            // Checking to make sure that the leve sheet is even loading properly... if it doesn't then we have issues
            if (LeveSheet != null)
            {
                foreach (var row in LeveSheet)
                {
                    uint leveId = row.RowId;
                    var leveAssignmentType = row.LeveAssignmentType.RowId;
                    uint jobId = JobIdConverter(leveAssignmentType); // This is here to turn it into a proper JobId, saves the hopping from the leveAssignment number for future things

                    if (CrafterLeveSheetJobs.Contains(jobId)) // Temporarily limiting this to to just fisher + crafters. 
                    {
                        // All the crafters + fisher uses this sheet, so it's easier to just seperate it into it's own tracker of sorts. 
                        // Mainly so I can just grab what I need specifically from this sheet
#if DEBUG
                        Utils.PluginVerbos($"Loading up ID: {leveId}");
#endif

                        string leveName = row.Name.ToString();
                        leveName = leveName.Replace("<nbsp>", " ");
                        leveName = leveName.Replace("<->", "");
                        uint leveLevel = row.ClassJobLevel;
                        int expReward = row.ExpReward.ToInt();
                        int gilReward = row.GilReward.ToInt();
                        int AllowanceCost = row.AllowanceCost.ToInt();

                        // The questID of the leve. Need this for another sheet but also, might be useful to check progress of other quest...
                        uint questID = row.DataId.RowId;
                        uint leveVendor = LeveVendor.FirstOrDefault(pair => pair.Value.Contains(leveId)).Key;
                        string vendorName = LeveNPCDict[leveVendor].Name;

                        uint leveClientId = row.LeveClient.Value.RowId;
                        uint turninNpcId = TurninNpcId(leveClientId, JobType: jobId);

                        var craftLeveInfo = CraftLeveSheet.Where(x => x.Leve.RowId == leveId).FirstOrDefault();
                        if (craftLeveInfo.RowId != 917504)
                        {
                            uint itemID = CraftLeveSheet.GetRow(questID).Item[0].Value.RowId;
                            var itemInfo = ItemSheet.GetRow(itemID);
                            string itemName = itemInfo.Name.ToString();
                            ISharedImmediateTexture? itemIcon = null;
                            if (Svc.Texture.TryGetFromGameIcon((int)itemInfo.Icon, out itemIcon)) { }
                            int leveRepeat = craftLeveInfo.Repeats.ToInt();
                            leveRepeat += 1;

                            int turninAmount = 0;
                            for (int x = 0; x < 3; x++)
                            {
                                turninAmount += craftLeveInfo.ItemCount[x].ToInt();
                            }
                            turninAmount = turninAmount * leveRepeat;
                            LeveDictionary.TryAdd(leveId, new LeveData()
                            {
                                LeveName = leveName,
                                JobId = jobId,
                                Level = leveLevel,
                                LeveVendorID = leveVendor,
                                TurninVendorId = turninNpcId,
                                QuestID = questID,
                                ExpReward = expReward,
                                GilReward = gilReward,
                                AllowanceCost = AllowanceCost,
                                CrafterInfo = new()
                                {
                                    LeveTurninVendorID = turninNpcId,
                                    RepeatAmount = leveRepeat,
                                    ItemID = itemID,
                                    ItemName = itemName,
                                    ItemIcon = itemIcon,
                                    TurninAmount = turninAmount,
                                },
                            });
                        }
                    }
                }
#if DEBUG
                Utils.PluginInfo($"Total Dictionary Count: {LeveInfo.LeveDictionary.Count}");
#endif
            }
        }

        private static uint JobIdConverter(uint leveJob)
        {
            uint jobId = 0;

            if (leveJob == 2) // Miner
                jobId = 16;
            else if (leveJob == 3) // BTN
                jobId = 17;
            else if (leveJob == 4) // FSH
                jobId = 18;
            else if (leveJob == 5) // CRP
                jobId = 8;
            else if (leveJob == 6) // BSM
                jobId = 9;
            else if (leveJob == 7) // ARM
                jobId = 10;
            else if (leveJob == 8) // GSM
                jobId = 11;
            else if (leveJob == 9) // LTW
                jobId = 12;
            else if (leveJob == 10) // WVR
                jobId = 13;
            else if (leveJob == 11) // ALC
                jobId = 14;
            else if (leveJob == 12) // CUL
                jobId = 15;

            return jobId;
        }
    }
}
