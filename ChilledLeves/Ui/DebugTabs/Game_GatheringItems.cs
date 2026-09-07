using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.GatheringHelper;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_GatheringItems
    {
        public static GatheringRule currentRule = GatheringRule.Search;

        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<Gathering>(out var gathering) && gathering.IsAddonReady)
            {
                DrawGatheringRuleCombo();

                var itemToGather = gathering.GatheredItems
                    .Where(x => x.ItemID != 0)
                    .OrderByDescending(x => x.GatherChance)
                    .ThenByDescending(x => x.ItemLevel)
                    .FirstOrDefault();

                if (itemToGather != null)
                {
                    ImGui.Text($"Checking item: {itemToGather.ItemName} | Boon: {itemToGather.BoonChance} | Gather: {itemToGather.GatherChance}");
                    if (ImGui.Button("Buff Info"))
                    {
                        IceLogging.Verbose("- - - - Buff Test - - - ", "Debug: Gather Buffs");
                        CheckGatheringBuffs(gathering, itemToGather);
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Clear"))
                {
                    foreach (var buff in CurrentUse)
                        CurrentUse[buff.Key] = 0;
                }

                DrawGatheringBuffUsage();
            }
            else
            {
                ImGui.Text("Gathering window isn't available... woops");
            }
        }

        private static void DrawGatheringRuleCombo()
        {
            if (ImGui.BeginCombo("Gathering Rule", currentRule.ToString()))
            {
                foreach (GatheringRule rule in Enum.GetValues<GatheringRule>())
                {
                    var isSelected = currentRule == rule;
                    if (ImGui.Selectable(rule.ToString(), isSelected))
                    {
                        currentRule = rule;
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
        }

        private static Gather_Enums BuffCounter = Gather_Enums.Unknown;

        private static Dictionary<Gather_Enums, int> CurrentUse = new()
        {
            [Gather_Enums.BoonIncrease_1] = 0,
            [Gather_Enums.BoonIncrease_2] = 0,
            [Gather_Enums.Tidings] = 0,
            [Gather_Enums.YieldI] = 0,
            [Gather_Enums.YieldII] = 0,
            [Gather_Enums.BonusIntegrity] = 0,
            [Gather_Enums.BonusIntegrity_Chance] = 0,
            [Gather_Enums.BYII] = 0,
            [Gather_Enums.FieldMasteryI] = 0,
            [Gather_Enums.FieldMasteryII] = 0,
            [Gather_Enums.FieldMasteryIII] = 0,
            [Gather_Enums.FieldMasteryTemp] = 0,
            [Gather_Enums.TwelveBounty] = 0,
            [Gather_Enums.GivingLand] = 0,
            [Gather_Enums.Scrutiny] = 0,
            [Gather_Enums.Focus] = 0,
            [Gather_Enums.Priming] = 0,
            [Gather_Enums.Scour] = 0,
            [Gather_Enums.Brazen] = 0,
            [Gather_Enums.Meticulous] = 0,
        };

        private static void DrawGatheringBuffUsage()
        {
            if (ImGui.CollapsingHeader("Gathering Buff Usage"))
            {
                if (ImGui.BeginTable("GatherBuffUsageTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Buff");
                    ImGui.TableSetupColumn("Uses");
                    ImGui.TableHeadersRow();

                    foreach (var (buff, count) in CurrentUse)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(buff.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(count.ToString());
                    }

                    ImGui.EndTable();
                }
            }
        }

        private static void CheckGatheringBuffs(Gathering gathering, Gathering.GatheredItem item)
        {
            string tag = "Debug: Gather Buffs";

            var rule = currentRule;
            var playerLevel = Player.Level;
            var job = Player.Job;
            var currentGp = Utils.GetGp();

            bool missingDur = gathering.CurrentIntegrity != gathering.TotalIntegrity;

            var profileId = C.RuleProfiles[rule];
            var gatherProfile = C.FindGatherProfile(profileId) ?? C.GatherProfiles[0];
            IceLogging.Verbose($"Rule {rule} -> profileId {profileId} -> resolved profile {gatherProfile.ProfileId} ({gatherProfile.Name})", tag);

            foreach (var buff in gatherProfile.BuffPriority)
            {
                var buffProfile = gatherProfile.GatheringBuffs[buff];
                var actionInfo = Gather_Util.gathActionDict[buff];
                var actionId = actionInfo.ClassAction[job].ActionId;
                var useCount = CurrentUse[buff];

                if (!buffProfile.Enabled)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not enabled", tag);
                    continue;
                }

                if (actionInfo.RequiredLv > playerLevel)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not not high enough lv", tag);
                    continue;
                }

                if (currentGp < actionInfo.RequiredGp)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not enough GP", tag);
                    continue;
                }

                if (buffProfile.MaxUse != 0 && buffProfile.MaxUse > useCount)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to max use not being 0: {buffProfile.MaxUse != 0} && maxUse being more than useCount", tag);
                    continue;
                }

                if (buff is Gather_Enums.BoonIncrease_1 or Gather_Enums.BoonIncrease_2)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    bool noBoonGain = item.BoonChance == 100;
                    bool minGp = buffProfile.GP_Min >= currentGp;

                    if (hasStatus || noBoonGain || missingDur)
                    {
                        IceLogging.Verbose($"Reporting you can't use: {buff}", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
                else if (buff is Gather_Enums.Tidings)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    if (hasStatus)
                    {
                        IceLogging.Verbose($"Reporting you can't use: {buff}", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
                else if (buff is Gather_Enums.YieldI or Gather_Enums.YieldII)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    bool minimumInteg = buffProfile.Durability_MinUse != 0 && gathering.TotalIntegrity < buffProfile.Durability_MinUse;

                    if (hasStatus || minimumInteg)
                    {
                        IceLogging.Verbose($"We can't use {buff}, continuing onwards | Status: {hasStatus} | Minimum Integrity. {minimumInteg} [{buffProfile.Durability_MinUse} > {gathering.TotalIntegrity}]", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
                else if (buff is Gather_Enums.BonusIntegrity)
                {
                    if (!missingDur)
                    {
                        IceLogging.Verbose($"We're not missing durability, so not using {buff}", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
                else if (buff is Gather_Enums.BonusIntegrity_Chance)
                {
                    bool missingStatus = !Utils.HasStatusId(actionInfo.StatusId);
                    if (missingStatus && !missingDur)
                    {
                        IceLogging.Verbose($"We don't have bonus integ status, or we're not missing durability, so not using {buff}", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
                else if (buff is Gather_Enums.BYII)
                {
                    var status1 = Utils.HasStatusId(actionInfo.StatusId);
                    var status2 = Utils.HasStatusId(actionInfo.StatusId2);

                    bool hasStatus = status1 || status2;
                    if (hasStatus)
                    {
                        IceLogging.Verbose($"Reporting you can't use {buff}", tag);
                        continue;
                    }

                    IceLogging.Verbose($"{buff} is valid to use", tag);
                    CurrentUse[buff]++;
                    // return true;
                }
            }
        }
    }
}
