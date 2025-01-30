using ECommons.Throttlers;
using ExplorersIcebox.Scheduler.Handers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskReturnToIsland
    {
        public static void Enqueue()
        {
            TaskTeleport.Enqueue(LowerLimsaAether, LowerLimsaZoneID);
            /*
            TaskMoveTo.Enqueue(new Vector3(173.11f, 14.1f, 668.94f), "In front of Baldin", false, 1);
            TaskInteract.Enqueue(BaldinNPCID);
            P.taskManager.Enqueue(() => EnterIsland());
            */
        }

        internal unsafe static bool? EnterIsland()
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("Talk", out var addon) && GenericHelpers.IsAddonReady(addon))
            {
                if (EzThrottler.Throttle("Talk Textbox", 500))
                GenericHandlers.FireCallback("Talk", true);
                return false;
            }

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectString", out var addon2) && GenericHelpers.IsAddonReady(addon2))
            {
                if (EzThrottler.Throttle("Yes I want to enter Island", 1000))
                    GenericHandlers.FireCallback("SelectString", true, 0);
                return false;
            }

            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>("SelectYesNo", out var addon3) && GenericHelpers.IsAddonReady(addon3))
            {
                if (EzThrottler.Throttle("Travel to Island", 1000))
                    GenericHandlers.FireCallback("SelectYesNo", true, 0);
                return true;
            }

            return false;
        }
    }
}
