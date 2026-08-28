using ChilledLeves.Enums;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Travel
    {
        public static void Grab_TravelEnqueue()
        {
            string tag = "Task Travel: Grab Enqueue";

            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                if (LeveInfo.LeveNpc_Info.TryGetValue(sheetInfo.Npc_Vendor, out var vendorInfo))
                {
                    P.taskManager.Enqueue(() => AethernetTask_Grab(vendorInfo), "Navmesh task Running");
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

        public static void Turnin_Enqueue()
        {
            string tag = "Task Travel: Turnin Enqueue";
            if (LeveInfo.Leve_SheetInfo.TryGetValue(Leve_Helper.LeveToGrab, out var sheetInfo))
            {
                if (LeveInfo.LeveNpc_Info.TryGetValue(sheetInfo.Npc_Turnin, out var vendorInfo))
                {
                    P.taskManager.Enqueue(() => AethernetTask_Turnin(vendorInfo), "Navmesh Task Running");
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

        public static bool AethernetTask_Grab(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Travel: Navmesh Check";

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                if (Player.DistanceTo(vendorInfo.Npc_InteractZone) < 2)
                {
                    IceLogging.Verbose("We're close enough to the interact zone we don't need to queue up anything. Continuing to grab leve", tag);
                    Leve_Helper.State = Leve_Helper.SelectedMode switch
                    {
                        ModeSelection.Standard => LeveState.Grab_StandardLeve,
                        ModeSelection.ARR_Grind => LeveState.Grab_ARRLeve,
                        _ => LeveState.Grab_StandardLeve
                    };
                    IceLogging.Info($"Exiting to grab our leves with the following mode: {Leve_Helper.State}", tag);
                    return true;
                }
                else
                {
                    IceLogging.Verbose("Queueing up navigation task", tag);
                    navTask.Enqueue(() => Task_Navmesh.TeleportCheck(vendorInfo));
                }
            }
            else
            {
                var count = navTask.Tasks.Count();

                if (EzThrottler.Throttle("Navigation is running", 2000))
                    IceLogging.Verbose($"Navigation is currently running, current task count: [{count}]", tag);
            }

            return false;
        }

        public static bool AethernetTask_Turnin(LeveInfo.VendorInfo vendorInfo)
        {
            const string tag = "Travel: Navmesh Check";

            var navTask = P.navTask;
            if (!navTask.IsBusy)
            {
                if (Player.DistanceTo(vendorInfo.Npc_InteractZone) < 2)
                {
                    IceLogging.Verbose("We're close enough to the interact zone we don't need to queue up anything. Continuing to grab leve", tag);
                    Leve_Helper.State = LeveState.Turnin_Leve;
                    IceLogging.Info($"Exiting to grab our leves with the following mode: {Leve_Helper.State}", tag);
                    return true;
                }
                else
                {
                    IceLogging.Verbose("Queueing up navigation task", tag);
                    navTask.Enqueue(() => Task_Navmesh.TeleportCheck(vendorInfo));
                }
            }
            else
            {
                var count = navTask.Tasks.Count();

                if (EzThrottler.Throttle("Navigation is running", 2000))
                    IceLogging.Verbose($"Navigation is currently running, current task count: [{count}]", tag);
            }

            return false;
        }
    }
}
