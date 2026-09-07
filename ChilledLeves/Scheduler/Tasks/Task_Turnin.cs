using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Turnin
    {
        private static List<uint> ActiveLeves = [];

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

            if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
            {
                if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
                {
                    var job = sheetInfo.Job;
                    if (Utils.Leve_IsAccepted(Leve_Helper.LeveToGrab))
                    {
                        if (LeveInfo.LeveJobs_Gathering.Contains(job))
                        {
                            SelectTurnin(selectString);
                        }
                        else
                        {
                            if (C.AllowMultiTurnin)
                                SelectMultiYes(selectString);
                            else
                                SelectMultiNo(selectString);
                        }

                        return false;
                    }
                    else if (EzThrottler.Throttle("Leaving post turnin"))
                    {
                        IceLogging.Verbose("Leaving the select string window", tag);
                        selectString.Entries.Last().Select();
                    }

                    return false;
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
                        IceLogging.Verbose("Selecting yes to the HQ prompt, or to denying multi turnin", tag);
                        selectYesNo.Yes();
                    }
                }
                else if (GenericHelpers.TryGetAddonMaster<JournalResult>(out var journalResult) && journalResult.IsAddonReady)
                {
                    if (EzThrottler.Throttle("Selecting yes to journal", 100))
                    {
                        IceLogging.Verbose("Selecting yes to the journal", tag);
                        journalResult.Complete();
                    }
                }
            }
            else if (Utils.Leve_IsAccepted(Leve_Helper.LeveToGrab))
            {
                var npcId = sheetInfo.Npc_Turnin;

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
                        IceLogging.Error($"NPC: {npcId} doesn't seem to exist in [{Player.Territory.RowId}]. " +
                                         $"Player Position: {Player.Position:N2}", tag);
                }
            }
            else
            {
                foreach (var leve in C.LeveOrder)
                {
                    if (!Utils.Leve_IsAccepted(leve))
                    {
                        C.LeveList[leve] -= 1;
                        C.SaveDebounced();
                    }
                }
                IceLogging.Verbose("We've completed our leve turnins for this round, going to see what state we need to be in next", tag);
                Leve_Helper.State = LeveState.CheckLeves;
                return true;
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

        private static bool SelectMultiYes(SelectString addon)
        {
            if (EzThrottler.Throttle("Selecting yes: SelectString"))
                addon.Entries[0].Select();
            return true;
        }

        private static bool SelectMultiNo(SelectString addon)
        {
            if (EzThrottler.Throttle("Selecting no: SelectString"))
                addon.Entries[1].Select();

            return true;
        }
    }
}
