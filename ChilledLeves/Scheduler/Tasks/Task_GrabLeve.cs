using ChilledLeves.Enums;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using SharpDX.DirectWrite;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_GrabLeve
    {
        public static void Enqueue_Standard()
        {
            string tag = "Task: Grab Leve";

            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                var firstVendor = sheetInfo.Npc_Vendors.First();

                if (LeveInfo.Levemete_Info.TryGetValue(firstVendor, out var vendorInfo))
                {
                    P.taskManager.EnqueueMulti
                    (
                        new(() => Task_Travel.AethernetTask_Grab(vendorInfo), "Traveling to vendor NPC"),
                        new(() => OpenLeveWindow(vendorInfo, firstVendor), "Opening Leve Menu"),
                        new(() => GrabLeve(), "Grabbing the leve from the vendor"),
                        new(() => CheckOtherLeves(), "Checking for multi leve grab"),
                        new(() => LeaveVendor(), "Leaving the leve Vendor")
                    );
                }
                else
                {
                    IceLogging.Error($"Missing NPC info on the following leve: {Leve_Helper.LeveToGrab}. Gave Id: {sheetInfo.Npc_Vendors.First()}", tag);
                    Leve_Helper.State = LeveState.Idle;
                }
            }
            else
            {
                IceLogging.Error($"We seem to be missing a leve out of the sheets? {Leve_Helper.LeveToGrab}. Please report back to me on this", tag);
                Leve_Helper.State = LeveState.Idle;
            }
        }

        public static void Enqueue_ARR()
        {
            string tag = "Task: Grab ARR Leve";
            var arrVendor = C.ARR_NpcId;


            if (LeveInfo.Levemete_Info.TryGetValue(arrVendor, out var vendorInfo))
            {
                P.taskManager.EnqueueMulti
                (
                    new(() => Task_Travel.AethernetTask_Grab(vendorInfo), "Traveling to vendor NPC"),
                    new(() => OpenLeveWindow(vendorInfo, arrVendor), "Opening Leve Menu"),
                    new(() => Grab_ARRLeve(), "Grabbing the leve from the vendor"),
                    new(() => LeaveVendor(), "Leaving the leve Vendor")
                );
            }
            else
            {
                IceLogging.Error($"Missing NPC info on the following leve: {Leve_Helper.LeveToGrab}. Gave Id: {arrVendor}", tag);
                Leve_Helper.State = LeveState.Idle;
            }
        }

        private static int talkCooldown = 0;

        private static bool OpenLeveWindow(LeveInfo.Info_Vendor npcInfo, uint npcId)
        {
            string tag = "Open Leve Window";

            if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
            {
                IceLogging.Debug("We should be in the journal tab now! So we're going to collect our leve", tag);
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<Talk>("Talk", out var talk) && talk.IsAddonReady)
            {
                if (EzThrottler.Throttle("Talk Window Visibility", 10))
                    talkCooldown += 1;

                if (talkCooldown > 1)
                {
                    talk.Click();
                    talkCooldown = 0;
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting Addon Button", 1000))
                    SelectLeveKind(selectString);
            }
            else
            {
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
        public static void SelectLeveKind(SelectString addon)
        {
            string tag = "Select Leve: Addon";

            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                var kind = sheetInfo.LeveType;

                string NormalizeForComparison(string input)
                {
                    return System.Text.RegularExpressions.Regex.Replace(input, @"\d+", "").Trim();
                }

                if (!LeveInfo.Leve_SelectText.TryGetValue(kind, out var targetText))
                {
                    if (EzThrottler.Throttle("Error Message Dictionary: LeveKind", 2000))
                        IceLogging.Error($"No text was found to match up in the dictionary. Please report this. {kind}", tag);
                    return;
                }

                var normalizedTarget = NormalizeForComparison(targetText);

                SelectString.Entry? match = addon.Entries.Cast<AddonMaster.SelectString.Entry?>()
                        .FirstOrDefault(e => NormalizeForComparison(e!.Value.Text)
                        .Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));

                if (match is null)
                {
                    if (EzThrottler.Throttle("Error Message: LeveKind", 2000))
                        IceLogging.Error($"No text was found to match up in the addon itself. Please report this. {kind}", tag);
                    return;
                }

                if (EzThrottler.Throttle("Positive Kind Message", 2000))
                    IceLogging.Verbose($"We managed to find a kind to match up! Selecting it now [{match.Value.Text}] {kind}", tag);
                match.Value.Select();
            }
            else
            {
                if (EzThrottler.Throttle("Error Message: Sheet Info", 2000))
                    IceLogging.Error($"Hey! We've somehow gotten an invalid leve that doesn't exist in the sheets... {Leve_Helper.LeveToGrab}", tag);
            }
        }
        public static bool GrabLeve()
        {
            string tag = "Task: Grab Leve";
            
            if (Utils.Leve_IsAccepted(Leve_Helper.LeveToGrab))
            {
                IceLogging.Debug("We've accepted the leve, continuing onto checking for multi leves", tag);

                Update_PotentionalMulti();

                return true;
            }

            if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
            {
                if (Leve_Helper.LeveToGrab == guildLeve.SelectedLeveId)
                {
                    if (EzThrottler.Throttle("Accepting Leve", 1000))
                        GenericHandlers.FireCallback("JournalDetail", true, 3, (int)Leve_Helper.LeveToGrab);

                    return false;
                }

                var goalLeve = LeveInfo.Leve_SheetInfo[Leve_Helper.LeveToGrab];
                var goalJob = goalLeve.Job;

                if (EzThrottler.Throttle("Current Status of Primary Leve", 1000))
                    IceLogging.Verbose($"Currently attempting to grab leve. Goal Leve: {Leve_Helper.LeveToGrab} | Job: {goalJob}", tag);

                if (!guildLeve.SelectJob(goalJob, goalLeve.Level))
                {
                    IceLogging.Verbose("We're on the wrong job tab, so going to fix that", tag);
                    return false;
                }

                if (LeveInfo.LoadedList().Contains(Leve_Helper.LeveToGrab))
                {
                    if (EzThrottler.Throttle("Selecting leve", 1000))
                    {
                        IceLogging.Verbose($"Selecting leve: {Leve_Helper.LeveToGrab}", tag);
                        guildLeve.SelectProperLeve(guildLeve, Leve_Helper.LeveToGrab);
                    }
                }
            }

            return false;
        }
        public static bool Grab_ARRLeve()
        {
            string tag = "Task: Grab ARR Leve";

            var arrList = Leve_Helper.ValidARRLeves;
            bool anyAccepted = arrList.Where(x => Utils.Leve_IsAccepted(x)).Any();

            if (anyAccepted)
            {
                IceLogging.Debug("We've accepted the leve for ARR Grind Mode, continuing to do said leve", tag);
                return true;
            }

            if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
            {
                var goalLeve = LeveInfo.Leve_SheetInfo[Leve_Helper.LeveToGrab];
                var goalJob = goalLeve.Job;

                if (!guildLeve.SelectJob(goalJob, goalLeve.Level))
                {
                    IceLogging.Verbose($"We're on the wrong job tab, so going to fix that. Job tab: {goalJob}", tag);
                    return false;
                }
                var loadedList = LeveInfo.LoadedList();

                // this kinda gets messy, so in case I need to review this later...
                // If we don't have a leve that we were told to grab (will happen on first passthrough intentionally
                if (Leve_Helper.ARRLeveToGrab == 0)
                {
                    // Checking the priority list to see if we have a leve that we can grab
                    // Checks for the top most prio (user table / first in list) -> the bottom of the listing
                    if (!loadedList.Any(x => LeveInfo.Leve_SheetInfo[x].Job == goalJob))
                    {
                        IceLogging.Verbose("We haven't... loaded the list with the job. Be patient you dev >.>", tag);
                        return false;
                    }

                    foreach (var leve in arrList)
                    {
                        if (!loadedList.Contains(leve))
                            continue;

                        IceLogging.Verbose("Found a leve that was highest on the priority list, so we're going for that", tag);
                        Leve_Helper.ARRLeveToGrab = leve;
                        break;
                    }

                    // If somehow we don't have a leve to grab (f for respects) we need to see if we're in the complete condition
                    // Otherwise, we can't do shit lol
                    if (Leve_Helper.ARRLeveToGrab == 0)
                    {
                        // If we're not in completion mode
                        if (C.ARR_StopCondition != StopConditions.Complete)
                        {
                            IceLogging.Error("We seem to have run out of leves to do, so we're going to stop the process", tag);
                            SchedulerMain.DisablePlugin();
                            return true;
                        }

                        IceLogging.Verbose("We need to refresh the listing to get a leve for us to complete, so we're going to find one", tag);

                        // We set the first leve that we could potentionally do in the beginning
                        // SO IDEALLY, this should refresh the list based on that...
                        var refreshLeve = loadedList
                            .Where(x =>
                            {
                                var info = LeveInfo.Leve_SheetInfo[x];
                                return info.JobAssignmentType == goalLeve.JobAssignmentType
                                    && info.Level == goalLeve.Level;
                            })
                            .Where(x => Utils.PotentionalLeve(x, tag))
                            .FirstOrDefault();

                        if (refreshLeve == 0)
                        {
                            IceLogging.Error("No visible leve matches the level of our goal leve, so we're going to stop the process", tag);
                            SchedulerMain.DisablePlugin();
                            return true;
                        }

                        IceLogging.Verbose($"Found leve {refreshLeve} at the goal level to refresh the listing", tag);
                        Leve_Helper.ARRLeveToGrab = refreshLeve;
                    }
                }
                else
                {
                    // We're in a point where we can grab a leve, so going to do so
                    if (Leve_Helper.ARRLeveToGrab == guildLeve.SelectedLeveId)
                    {
                        if (EzThrottler.Throttle("Accepting Leve", 1000))
                            GenericHandlers.FireCallback("JournalDetail", true, 3, (int)Leve_Helper.ARRLeveToGrab);

                        return false;
                    }

                    if (EzThrottler.Throttle("Current Status of Primary Leve", 1000))
                        IceLogging.Verbose($"Currently attempting to grab leve. Goal Leve: {Leve_Helper.ARRLeveToGrab} | Job: {goalJob}", tag);

                    if (LeveInfo.LoadedList().Contains(Leve_Helper.ARRLeveToGrab))
                    {
                        if (EzThrottler.Throttle("Selecting leve", 1000))
                        {
                            IceLogging.Verbose($"Selecting leve: {Leve_Helper.ARRLeveToGrab}", tag);
                            guildLeve.SelectProperLeve(guildLeve, Leve_Helper.ARRLeveToGrab);
                        }
                    }
                }
            }

            return false;
        }

        private static List<uint> ValidLeves = new();
        private static int ValidAmount = 0;
        private static int LastCost = 0;

        private static void Update_PotentionalMulti()
        {
            string tag = "Check MultiLeve";

            var lastLeveInfo = LeveInfo.Leve_SheetInfo[Leve_Helper.LeveToGrab];

            var currentNpcId = lastLeveInfo.Npc_Vendors.First();
            var leveList = C.LeveOrder;

            ValidLeves = null;
            ValidLeves = new();
            ValidAmount = Utils.Allowances;
            LastCost = lastLeveInfo.AllowanceCost;

            foreach (var leve in leveList)
            {
                if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo))
                {
                    if (sheetInfo.Npc_Vendors.First() != currentNpcId)
                    {
                        IceLogging.Verbose($"Leve: {leve} | Not the same npc", tag);
                        continue;
                    }

                    if (sheetInfo.AllowanceCost != LastCost)
                    {
                        IceLogging.Verbose($"Leve: {leve} | Not the same cost", tag);
                        continue;
                    }

                    bool enoughLeves = ValidAmount >= sheetInfo.AllowanceCost;

                    if (Utils.PotentionalLeve(leve, tag) && enoughLeves)
                    {
                        ValidLeves.Add(leve);
                        ValidAmount -= sheetInfo.AllowanceCost;
                    }
                }
            }
            IceLogging.Verbose($"Exiting the update multi with the following potentional leves: {ValidLeves.Count()}", tag);
        }
        private static bool CheckOtherLeves()
        {
            string tag = "Debug: Check Multi Leves";

            if (C.GrabMulti)
            {
                if (Utils.Leve_MaxAccepted())
                {
                    IceLogging.Verbose("We've reached the cap on accepted leves, going to stop here and continue on with the process", tag);
                }
                else if (ValidLeves.FirstOrDefault(x => !Utils.Leve_IsAccepted(x)) is var multiLeve && multiLeve != 0)
                {
                    if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
                    {
                        if (multiLeve == guildLeve.SelectedLeveId)
                        {
                            if (EzThrottler.Throttle("Grabbing Multi-Leve"))
                            {
                                GenericHandlers.FireCallback("JournalDetail", true, 3, (int)multiLeve);
                            }

                            return false;
                        }

                        var goalLeve = LeveInfo.Leve_SheetInfo[multiLeve];
                        var goalJob = goalLeve.Job;

                        if (EzThrottler.Throttle("Current Status of Multi Leve", 1000))
                            IceLogging.Verbose($"Currently attempting to grab leve. Goal Leve: {multiLeve} | Job: {goalJob}", tag);

                        if (!guildLeve.SelectJob(goalJob, goalLeve.Level))
                        {
                            IceLogging.Verbose("We're on the wrong job tab, so going to fix that", tag);
                            return false;
                        }

                        if (LeveInfo.LoadedList().Contains(multiLeve))
                        {
                            if (EzThrottler.Throttle("Selecting leve", 1000))
                            {
                                IceLogging.Verbose($"Selecting leve: {multiLeve}", tag);
                                guildLeve.SelectProperLeve(guildLeve, multiLeve);
                            }
                        }
                    }
                }
                else
                {
                    IceLogging.Verbose("We have grabbed all potentional leves from this vendor, continuing on", tag);
                    return true;
                }
            }
            else
            {
                IceLogging.Verbose("We were told that Grab Multi-Leves was disabled, so we're going to stop", tag);
                return true;
            }

            return false;
        }
        private static bool LeaveVendor()
        {
            const string tag = "Grab Leve: Leave Vendor";

            if (!Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
            {
                IceLogging.Verbose("We're finally free of those leves! Starting fresh so we can figure out where to go", tag);
                Leve_Helper.State = LeveState.CheckLeves;

                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<GuildLeve>(out var guildLeve) && guildLeve.IsAddonReady)
            {
                if (EzThrottler.Throttle("Closing Leve Window", 100))
                {
                    IceLogging.Verbose("Guildleve window [Leve Selection] was still open. Closing", tag);
                    guildLeve.Close(guildLeve);
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectString>(out var selectString) && selectString.IsAddonReady)
            {
                var exit = selectString.Entries.Last();
                if (EzThrottler.Throttle("Closing SelectString", 100))
                {
                    IceLogging.Verbose("SelectString window was still open, closing", tag);
                    exit.Select();
                }
            }

            return false;
        }
    }
}
