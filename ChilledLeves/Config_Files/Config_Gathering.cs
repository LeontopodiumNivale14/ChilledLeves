using ChilledLeves.Enums;
using ChilledLeves.Utilities.GatheringHelper;
using System.Collections.Generic;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public class GatherProfile
    {
        public int ProfileId { get; set; }
        public string Name { get; set; } = "New Profile";
        public uint GP_MinimumInteraction { get; set; } = 0;
        public Dictionary<Gather_Enums, BuffSettings> GatheringBuffs { get; set; } = new()
        {
            [Gather_Enums.BoonIncrease_1] = new(),
            [Gather_Enums.BoonIncrease_2] = new(),
            [Gather_Enums.Tidings] = new(),
            [Gather_Enums.YieldI] = new(),
            [Gather_Enums.YieldII] = new(),
            [Gather_Enums.BonusIntegrity] = new(),
            [Gather_Enums.BonusIntegrity_Chance] = new(),
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
        };

        public List<Gather_Enums> BuffPriority { get; set; } = new()
        {
            Gather_Enums.BoonIncrease_1, Gather_Enums.BoonIncrease_2,
            Gather_Enums.Tidings, Gather_Enums.YieldI, Gather_Enums.YieldII,
            Gather_Enums.BonusIntegrity, Gather_Enums.BonusIntegrity_Chance,
            Gather_Enums.BYII, Gather_Enums.FieldMasteryI, Gather_Enums.FieldMasteryII, Gather_Enums.FieldMasteryIII, Gather_Enums.FieldMasteryTemp,
            Gather_Enums.TwelveBounty, Gather_Enums.Scrutiny,
        };

        public GatherProfile() { }

        public GatherProfile(GatherProfile other)
        {
            ProfileId = other.ProfileId;
            Name = other.Name;
            GP_MinimumInteraction = other.GP_MinimumInteraction;
            GatheringBuffs = other.GatheringBuffs.ToDictionary(
                kv => kv.Key,
                kv => new BuffSettings
                {
                    Enabled = kv.Value.Enabled,
                    GP_Min = kv.Value.GP_Min,
                    MaxUse = kv.Value.MaxUse,
                    Durability_MinUse = kv.Value.Durability_MinUse,
                    MinItems = kv.Value.MinItems,
                });
            BuffPriority = new List<Gather_Enums>(other.BuffPriority);
        }
    }

    public Dictionary<GatheringRule, int> RuleProfiles { get; set; } = new()
    {
        [GatheringRule.Search] = 1,
        [GatheringRule.Procurance] = 2,
        [GatheringRule.Search_Procurance] = 3,
        [GatheringRule.Execution] = 4,
    };

    public class BuffSettings
    {
        public bool Enabled { get; set; } = false;
        public uint GP_Min { get; set; } = 0;
        public uint MaxUse { get; set; } = 0;
        public uint Durability_MinUse { get; set; } = 0;
        public uint MinItems { get; set; } = 0;
    }

    public List<GatherProfile> GatherProfiles { get; set; } = new()
    {
        Gather_Util.DefaultProfile,
        Gather_Util.Type_Search,
        Gather_Util.Type_Procure,
        Gather_Util.Type_Execute,
        Gather_Util.Type_Search_Procure,
    };

    public void AddNewGatheringProfile()
    {
        var rng = new Random();
        int profileId = rng.Next(1, 50000);
        while (FindGatherProfile(profileId) != null)
            profileId = rng.Next(1, 50000);

        GatherProfiles.Add(new GatherProfile
        {
            ProfileId = profileId
        });
        SaveDebounced();
    }

    public GatherProfile? FindGatherProfile(int id) => GatherProfiles.Find(c => c.ProfileId == id);
}
