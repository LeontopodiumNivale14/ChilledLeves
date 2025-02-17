using ChilledLeves.Scheduler.Handers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskUseAethernet
    {
        public static void Enqueue(uint ZoneID)
        {
            P.taskManager.Enqueue(() => PlayerNotBusy());
            if (ZoneID == 129 || ZoneID == 128)
            {
                TaskMoveTo.Enqueue(AethernetDict[ZoneID].CrystalLoc, "Aetheryte in the City", false, 5);
                TaskTarget.Enqueue(AethernetDict[ZoneID].AetheryteID);
                TaskInteract.Enqueue(AethernetDict[ZoneID].AetheryteID);
                P.taskManager.Enqueue(() => LimsaAethernet(ZoneID), $"Heading to: {ZoneID} in Limsa");
            }
        }

        internal static unsafe bool? LimsaAethernet(uint ZoneID)
        {
            var goalZone = AethernetDict[ZoneID].TeleportZone;
            var aethernetSelect = (int)AethernetDict[ZoneID].Aethernet;

            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var SelectString) && IsAddonReady(SelectString))
            {
                if (EzThrottler.Throttle("Opening Aethernet", 500))
                {
                    GenericHandlers.FireCallback("SelectString", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("TelepotTown", out var TelepotTown) && IsAddonReady(TelepotTown))
            {
                if (EzThrottler.Throttle("Going to Aethernet Spot", 500))
                {
                    GenericHandlers.FireCallback("TelepotTown", true, 11, aethernetSelect);
                }
            }
            else if (IsInZone(goalZone) && IsScreenReady())
            {
                PluginLog($"Is in zone: {goalZone}");
                return true;
            }

            return false;
        }
    }
}
