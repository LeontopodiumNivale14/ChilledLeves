using ChilledLeves.Enums;
using Dalamud.Interface.Textures;
using ECommons.ExcelServices;
using System;
using System.Collections.Generic;
using System.Text;
using static ChilledLeves.Config_Files.Config;

namespace ChilledLeves.Utilities.GatheringHelper;

public static partial class Gather_Util
{
    public class GatheringActions
    {
        /// <summary>
        /// Internal name for myself to know wtf this is
        /// </summary>
        public string internalName { get; set; }
        public Dictionary<Job, ClassDetails> ClassAction { get; set; } = new();
        public uint StatusId { get; set; } = 0;
        public uint StatusId2 { get; set; } = 0;
        public string StatusName { get; set; } = "??";
        public int RequiredGp { get; set; } = 0;
        public int RequiredLv { get; set; } = 0;
    }

    public class ClassDetails
    {
        public uint ActionId { get; set; }
        public string Name { get; set; }
        public uint IconId { get; set; }
        public ISharedImmediateTexture Icon { get; set; }
    }

    public static void Update_GatheringDetails()
    {
        foreach (var entry in gathActionDict)
        {
            foreach (var job in entry.Value.ClassAction)
            {
                if (ExcelHelper.Sheet_Action.TryGetRow(job.Value.ActionId, out var actionSheet))
                {
                    job.Value.Name = actionSheet.Name.ToString();
                    if (actionSheet.Icon is { } iconId && Svc.Texture.TryGetFromGameIcon((int)iconId, out var icon))
                    {
                        job.Value.IconId = iconId;
                        job.Value.Icon = icon;
                    }
                }
            }
        }
    }

    public static Dictionary<Gather_Enums, GatheringActions> gathActionDict = new()
    {
        [Gather_Enums.BoonIncrease_1] = new()
        {
            internalName = "Pioneer's Gift I",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 21177, Name = "Pioneer's Gift I" },
                [Job.BTN] = new() { ActionId = 21178, Name = "Pioneer's Gift I" },
            },
            StatusId = 2666,
            StatusName = "Gift of the Land",
            RequiredGp = 50,
            RequiredLv = 15,
        },
        [Gather_Enums.BoonIncrease_2] = new()
        {
            internalName = "Pioneer's Gift II",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 25589, Name = "Pioneer's Gift II" },
                [Job.BTN] = new() { ActionId = 25590, Name = "Pioneer's Gift II" },
            },
            StatusId = 759,
            StatusName = "Gift of the Land II",
            RequiredGp = 100,
            RequiredLv = 50,
        },
        [Gather_Enums.Tidings] = new()
        {
            internalName = "Nophica's Tidings",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 21203, Name = "Nophica's Tidings" },
                [Job.BTN] = new() { ActionId = 21204, Name = "Nophica's Tidings" },
            },
            StatusId = 2667,
            StatusName = "Gatherer's Bounty",
            RequiredGp = 200,
            RequiredLv = 81,
        },
        [Gather_Enums.YieldI] = new()
        {
            internalName = "Blessed Harvest",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 239, Name = "Blessed Harvest" },
                [Job.BTN] = new() { ActionId = 222, Name = "Blessed Harvest" },
            },
            StatusId = 219,
            StatusName = "Gathering Yield Up",
            RequiredGp = 400,
            RequiredLv = 30,
        },
        [Gather_Enums.YieldII] = new()
        {
            internalName = "Blessed Harvest II",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 241, Name = "Blessed Harvest II" },
                [Job.BTN] = new() { ActionId = 224, Name = "Blessed Harvest II" },
            },
            StatusId = 219,
            StatusName = "Gathering Yield Up",
            RequiredGp = 500,
            RequiredLv = 40,
        },
        [Gather_Enums.BonusIntegrity] = new()
        {
            internalName = "Ageless Words",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 232, Name = "Ageless Words" },
                [Job.BTN] = new() { ActionId = 215, Name = "Ageless Words" },
            },
            RequiredGp = 300,
            RequiredLv = 30,
        },
        [Gather_Enums.BonusIntegrity_Chance] = new()
        {
            internalName = "Wise of the World",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 26521, Name = "Wise of the World" },
                [Job.BTN] = new() { ActionId = 26522, Name = "Wise of the World" },
            },
            StatusId = 2765,
            StatusName = "",
            RequiredGp = 0,
            RequiredLv = 90,
        },
        [Gather_Enums.BYII] = new()
        {
            internalName = "Bountiful Yield/Harvest II",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 272, Name = "Bountiful Yield/Harvest II" },
                [Job.BTN] = new() { ActionId = 273, Name = "Bountiful Yield/Harvest II" },
            },
            StatusId = 1286,
            StatusId2 = 756,
            StatusName = "",
            RequiredGp = 100,
            RequiredLv = 68,
        },
        [Gather_Enums.FieldMasteryIII] = new()
        {
            // 50% increase
            internalName = "Field Mastery III",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 295, Name = "Field Mastery III" },
                [Job.BTN] = new() { ActionId = 294, Name = "Field Mastery III" },
            },
            StatusId = 218,
            StatusName = "Gathering Rate Up",
            RequiredGp = 250,
            RequiredLv = 10,
        },
        [Gather_Enums.FieldMasteryII] = new()
        {
            // 15% increase
            internalName = "Field Mastery II",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 237, Name = "Field Mastery II" },
                [Job.BTN] = new() { ActionId = 220, Name = "Field Mastery II" },
            },
            StatusId = 218,
            StatusName = "Gathering Rate Up",
            RequiredGp = 100,
            RequiredLv = 5,
        },
        [Gather_Enums.FieldMasteryI] = new()
        {
            // 5% increase
            internalName = "Field Mastery I",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 235, Name = "Field Mastery I" },
                [Job.BTN] = new() { ActionId = 218, Name = "Field Mastery I" },
            },
            StatusId = 218,
            StatusName = "Gathering Rate Up",
            RequiredGp = 50,
            RequiredLv = 4,
        },
        [Gather_Enums.FieldMasteryTemp] = new()
        {
            // 15% increase [temp]
            internalName = "Clear Vision | Flora Mastery",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 4072, Name = "Clear Vision" },
                [Job.BTN] = new() { ActionId = 4086, Name = "Flora Mastery" },
            },
            StatusId = 754,
            StatusName = "Gathering Rate Up (Limited)",
            RequiredGp = 50,
            RequiredLv = 23,
        },

        // Crystal Specific Actions

        [Gather_Enums.TwelveBounty] = new() // +3 yield on crystals, buff through node
        {
            internalName = "The Twelve Bounty",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 280, Name = "The Twelve Bounty" },
                [Job.BTN] = new() { ActionId = 282, Name = "The Twelve Bounty" },
            },
            StatusId = 825,
            StatusName = "The Twelve's Bounty",
            RequiredGp = 150,
            RequiredLv = 20,
        },
        [Gather_Enums.GivingLand] = new() // increase yield on crystal, random amount gained
        {
            internalName = "The Giving Land",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 4589, Name = "The Giving Land" },
                [Job.BTN] = new() { ActionId = 4590, Name = "The Giving Land" },
            },
            StatusId = 1802,
            StatusName = "The Giving Land",
            RequiredGp = 200,
            RequiredLv = 74,
        },

        // Collectable Buffs

        [Gather_Enums.Scrutiny] = new()
        {
            internalName = "Scrutiny",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 22185, Name = "Scrutiny" },
                [Job.BTN] = new() { ActionId = 22189, Name = "Scrutiny" },
            },
            StatusId = 757,
            StatusName = "",
            RequiredGp = 200,
        },
        [Gather_Enums.Focus] = new()
        {
            internalName = "Collector's Focus",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 21205, Name = "Collector's Focus" },
                [Job.BTN] = new() { ActionId = 21206, Name = "Collector's Focus" },
            },
            StatusId = 2668,
            StatusName = "",
            RequiredGp = 100,
        },
        [Gather_Enums.Priming] = new()
        {
            internalName = "Priming Touch",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 34871, Name = "Priming Touch" },
                [Job.BTN] = new() { ActionId = 34872, Name = "Priming Touch" },
            },
            StatusId = 2668,
            StatusName = "",
            RequiredGp = 100,
        },

        // Collectable Actions
        [Gather_Enums.Scour] = new()
        {
            // Base general use skill
            internalName = "Scour",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 22182, Name = "Scour" },
                [Job.BTN] = new() { ActionId = 22186, Name = "Scour" },
            },
            StatusId = 0,
            StatusName = "n/a",
            RequiredGp = 0,
        },
        [Gather_Enums.Brazen] = new()
        {
            // 50 - 150% buff
            internalName = "Brazen Woodsman",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 22183, Name = "Brazen Prospector" },
                [Job.BTN] = new() { ActionId = 22187, Name = "Brazen Woodsman" },
            },
            StatusId = 0,
            StatusName = "n/a",
            RequiredGp = 0,
        },
        [Gather_Enums.Meticulous] = new()
        {
            // Chance to not use durability/integrity
            internalName = "Meticulous Woodsman",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 22184, Name = "Meticulous Prospector" },
                [Job.BTN] = new() { ActionId = 22188, Name = "Meticulous Woodsman" },
            },
            StatusId = 0,
            StatusName = "n/a",
            RequiredGp = 0,
        },
        [Gather_Enums.Collect] = new()
        {
            internalName = "Collect",
            ClassAction = new()
            {
                [Job.MIN] = new() { ActionId = 240, Name = "Collect" },
                [Job.BTN] = new() { ActionId = 815, Name = "Collect" },
            },
            StatusId = 0,
            StatusName = "",
            RequiredGp = 0,
        },
    };

    public static GatherProfile DefaultProfile = new()
    {
        ProfileId = 0,
        Name = "Default",
    };

    public static GatherProfile Type_Search = new()
    {
        ProfileId = 1,
        Name = "Type: Search",
        GatheringBuffs = new()
        {
            [Gather_Enums.BonusIntegrity] = new() { Enabled = true },
            [Gather_Enums.BonusIntegrity_Chance] = new() { Enabled = true },

            [Gather_Enums.BoonIncrease_1] = new(),
            [Gather_Enums.BoonIncrease_2] = new(),
            [Gather_Enums.Tidings] = new(),
            [Gather_Enums.YieldI] = new(),
            [Gather_Enums.YieldII] = new(),
            [Gather_Enums.BYII] = new(),
            [Gather_Enums.FieldMasteryI] = new(),
            [Gather_Enums.FieldMasteryII] = new(),
            [Gather_Enums.FieldMasteryIII] = new(),
            [Gather_Enums.FieldMasteryTemp] = new(),
            [Gather_Enums.TwelveBounty] = new(),
            [Gather_Enums.GivingLand] = new(),
            [Gather_Enums.Scrutiny] = new(),
            [Gather_Enums.Focus] = new(),
            [Gather_Enums.Priming] = new(),
            [Gather_Enums.Scour] = new(),
            [Gather_Enums.Brazen] = new(),
            [Gather_Enums.Meticulous] = new(),
        }
    };

    public static GatherProfile Type_Procure = new()
    {
        ProfileId = 2,
        Name = "Type: Procure",
        GatheringBuffs = new()
        {
            [Gather_Enums.BoonIncrease_1] = new(),
            [Gather_Enums.BoonIncrease_2] = new(),
            [Gather_Enums.Tidings] = new(),
            [Gather_Enums.BonusIntegrity] = new() { Enabled = true },
            [Gather_Enums.BonusIntegrity_Chance] = new() { Enabled = true },
            [Gather_Enums.FieldMasteryI] = new(),
            [Gather_Enums.FieldMasteryII] = new(),
            [Gather_Enums.FieldMasteryIII] = new(),
            [Gather_Enums.FieldMasteryTemp] = new(),
            [Gather_Enums.TwelveBounty] = new(),
            [Gather_Enums.GivingLand] = new(),
            [Gather_Enums.Scrutiny] = new(),
            [Gather_Enums.Focus] = new(),
            [Gather_Enums.Priming] = new(),
            [Gather_Enums.Scour] = new(),
            [Gather_Enums.Brazen] = new(),
            [Gather_Enums.Meticulous] = new(),

            [Gather_Enums.BYII] = new() { Enabled = true },
            [Gather_Enums.YieldII] = new() { Enabled = true },
            [Gather_Enums.YieldI] = new() { Enabled = true }
        }
    };

    public static GatherProfile Type_Execute = new()
    {
        ProfileId = 4,
        Name = "Type: Execute",
        GatheringBuffs = new()
        {
            [Gather_Enums.BonusIntegrity] = new() { Enabled = true },
            [Gather_Enums.BonusIntegrity_Chance] = new() { Enabled = true },

            [Gather_Enums.BoonIncrease_1] = new(),
            [Gather_Enums.BoonIncrease_2] = new(),
            [Gather_Enums.Tidings] = new(),
            [Gather_Enums.YieldI] = new(),
            [Gather_Enums.YieldII] = new(),
            [Gather_Enums.BYII] = new(),
            [Gather_Enums.FieldMasteryI] = new(),
            [Gather_Enums.FieldMasteryII] = new(),
            [Gather_Enums.FieldMasteryIII] = new(),
            [Gather_Enums.FieldMasteryTemp] = new(),
            [Gather_Enums.TwelveBounty] = new(),
            [Gather_Enums.GivingLand] = new(),
            [Gather_Enums.Scrutiny] = new(),
            [Gather_Enums.Focus] = new(),
            [Gather_Enums.Priming] = new(),
            [Gather_Enums.Scour] = new(),
            [Gather_Enums.Brazen] = new(),
            [Gather_Enums.Meticulous] = new(),
        }
    };

    public static GatherProfile Type_Search_Procure = new()
    {
        ProfileId = 3,
        Name = "Type: Search & Procure",
        GatheringBuffs = new()
        {
            [Gather_Enums.BonusIntegrity] = new() { Enabled = true },
            [Gather_Enums.BonusIntegrity_Chance] = new() { Enabled = true },

            [Gather_Enums.BoonIncrease_1] = new(),
            [Gather_Enums.BoonIncrease_2] = new(),
            [Gather_Enums.Tidings] = new(),
            [Gather_Enums.YieldI] = new(),
            [Gather_Enums.YieldII] = new(),
            [Gather_Enums.BYII] = new(),
            [Gather_Enums.FieldMasteryI] = new(),
            [Gather_Enums.FieldMasteryII] = new(),
            [Gather_Enums.FieldMasteryIII] = new(),
            [Gather_Enums.FieldMasteryTemp] = new(),
            [Gather_Enums.TwelveBounty] = new(),
            [Gather_Enums.GivingLand] = new(),
            [Gather_Enums.Scrutiny] = new(),
            [Gather_Enums.Focus] = new(),
            [Gather_Enums.Priming] = new(),
            [Gather_Enums.Scour] = new(),
            [Gather_Enums.Brazen] = new(),
            [Gather_Enums.Meticulous] = new(),
        }
    };
}
