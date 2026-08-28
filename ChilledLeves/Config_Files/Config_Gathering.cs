using ChilledLeves.Enums;
using System;
using System.Collections.Generic;
using System.Text;

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
    }

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
        new()
        {
            ProfileId = 0,
            Name = "Default",
        }
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
