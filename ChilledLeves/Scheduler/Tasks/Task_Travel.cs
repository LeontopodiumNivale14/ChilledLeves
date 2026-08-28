using ChilledLeves.Enums;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using ECommons.Throttlers;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_Travel
    {
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

        public static bool Aethernet_GatheringTravel()
        {
            const string tag = "Travel: Gathering Pathing";

            return false;
        }
    }
}
