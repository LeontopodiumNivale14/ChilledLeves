using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Turnin
    {
        public static void Enqueue()
        {
            string tag = "Task: Turnin Leve";

            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                if (LeveInfo.LeveNpc_Info.TryGetValue(sheetInfo.Npc_Turnin, out var vendorInfo))
                {
                    P.taskManager.EnqueueMulti
                    (
                        new(() => Task_Travel.AethernetTask_Turnin(vendorInfo), "Pathing to turnin Vendor"),
                        new(() => TryTurnin(vendorInfo, sheetInfo), tag)
                    );
                }
                else
                {
                    IceLogging.Error($"Missing NPC info on the following leve: {Leve_Helper.LeveToGrab}. Gave Id: {sheetInfo.Npc_Vendor}", tag);
                    Leve_Helper.State = LeveState.Idle;
                }
            }
            else
            {
                IceLogging.Error($"We seem to be missing a leve out of the sheets? {Leve_Helper.LeveToGrab}. Please report back to me on this", tag);
                Leve_Helper.State = LeveState.Idle;
            }
        } 

        private static bool TryTurnin(LeveInfo.VendorInfo vendorInfo, LeveInfo.Leve_SheetData sheetInfo)
        {
            const string tag = "Turnin: Trying Turnin";
            var leveId = Leve_Helper.LeveToGrab;

            if (!Utils.Leve_IsAccepted(leveId))
            {
                if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Leaving post turnin"))
                    {
                        selectString.Entries.Last().Select();
                    }

                    return false;
                }

                if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
                {
                    if (EzThrottler.Throttle("Occupido by quest to turnin"))
                        IceLogging.Verbose("We're still interacting with the npc. Going to just wait", tag);

                    return false;
                }

                IceLogging.Verbose("We have completed our turnin. Going to start fresh to see what state we need to be in", tag);
                Leve_Helper.State = LeveState.CheckLeves;
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectIconString>(out var selectIconString) && selectIconString.IsAddonReady)
            {
                var match = selectIconString.Entries.Where(x => x.Text.Trim() == sheetInfo.LeveName.Trim()).ToArray();
                if (match.Length == 0)
                {
                    if (EzThrottler.Throttle("Error Log woops"))
                    {
                        IceLogging.Error("We seem to be missing the leve from this listing? (Atleast with multiple existing", tag);
                        IceLogging.Error("If you're running in a different language, please let me know", tag);
                        IceLogging.Error($"LeveID it failed to find: {leveId}. Name: {sheetInfo.LeveName}", tag);
                    }
                    Leve_Helper.State = LeveState.Idle;
                    return true;
                }
                else
                {
                    if (EzThrottler.Throttle("Selecting leve"))
                    {
                        IceLogging.Verbose("We were prompted to turnin multiple leves, so we're choosing the correct one (hopefully)", tag);
                        match[0].Select();
                    }
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>(out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Talk throttle", 10))
                {
                    IceLogging.Verbose("Skipping through talking dialog", tag);
                    talk.Click();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectYesno>(out var selectYesNo) && selectYesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes to HQ"))
                {
                    IceLogging.Verbose("Selecting yes to the HQ prompt", tag);
                    selectYesNo.Yes();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<JournalResult>(out var journalResult) && journalResult.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes to journal", 500))
                {
                    IceLogging.Verbose("Selecting yes to the journal", tag);
                    journalResult.Complete();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
            {
                if (SelectTurnin(selectString))
                {
                    return false;
                }
                else
                {
                    if (EzThrottler.Throttle("Multi turnin option"))
                    {
                        if (C.AllowMultiTurnin)
                        {
                            // selectString.
                        }
                        else
                        {

                        }
                    }
                }
            }
            else
            {
                var npcId = sheetInfo.Npc_Turnin;

                if (!Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
                {
                    if (Utils.TryGetObjectByDataId(npcId, out var gameObject))
                    {
                        if (EzThrottler.Throttle("Interact/Target NPC"))
                        {
                            Utils.TargetgameObject(gameObject);
                            Utils.InteractWithObject(gameObject);
                        }
                    }
                    else
                    {
                        if (EzThrottler.Throttle("Npc doesn't exist log", 2000))
                            IceLogging.Error($"NPC: {npcId} doesn't seem to exist in [{Player.Territory.RowId}].\n" +
                                             $"Player Position: {Player.Position:N2}", tag);
                    }
                }
            }

            return false;
        }

        public static bool SelectTurnin(SelectString addon)
        {
            string tag = "Select Leve: Addon";

            var kind = LeveKind.TurninLeve;

            string NormalizeForComparison(string input)
            {
                return System.Text.RegularExpressions.Regex.Replace(input, @"\d+", "").Trim();
            }

            if (!LeveInfo.Leve_SelectText.TryGetValue(kind, out var targetText))
            {
                return false;
            }

            var normalizedTarget = NormalizeForComparison(targetText);

            SelectString.Entry? match = addon.Entries.Cast<AddonMaster.SelectString.Entry?>()
                    .FirstOrDefault(e => NormalizeForComparison(e!.Value.Text)
                    .Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                return false;
            }

            if (EzThrottler.Throttle("Positive Kind Message", 2000))
            {
                IceLogging.Verbose($"We managed to find a kind to match up! Selecting it now [{match.Value.Text}] {kind}", tag);
                match.Value.Select();
            }
            return true;
        }
    }
}
