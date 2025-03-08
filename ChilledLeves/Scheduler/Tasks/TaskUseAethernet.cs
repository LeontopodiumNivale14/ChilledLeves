using ChilledLeves.Scheduler.Handlers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

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
            else if (ZoneID == 130 || ZoneID == 819)
            {
                Vector3 MainCityAether = AethernetDict[ZoneID].CrystalLoc;
                uint aetheryte = AethernetDict[ZoneID].AetheryteID;

                TaskMoveTo.Enqueue(MainCityAether, "Main Cities Aetheryte", false, 5);
                TaskTarget.Enqueue(aetheryte);
                TaskInteract.Enqueue(aetheryte);
                P.taskManager.Enqueue(() => OtherAethernet(ZoneID));
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
                if (EzThrottler.Throttle("Going to Aethernet Spot", 100))
                {
                    GenericHandlers.FireCallback("TelepotTown", true, 11, aethernetSelect);
                }
            }
            else if (IsInZone(goalZone) && IsScreenReady())
            {
                PluginVerbos($"Is in zone: {goalZone}");
                return true;
            }

            return false;
        }

        internal static unsafe bool? OtherAethernet(uint ZoneID)
        {
            Vector3 teleportSpot = AethernetDict[ZoneID].TPLocation;
            float teleportRange = AethernetDict[ZoneID].Range;
            int aethernetSelect = (int)AethernetDict[ZoneID].Aethernet;

            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var SelectString) && IsAddonReady(SelectString))
            {
                if (EzThrottler.Throttle("Opening Aethernet", 500))
                {
                    GenericHandlers.FireCallback("SelectString", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("TelepotTown", out var TelepotTown) && IsAddonReady(TelepotTown))
            {
                if (EzThrottler.Throttle("Going to Aethernet Spot", 100))
                {
                    GenericHandlers.FireCallback("TelepotTown", true, 11, aethernetSelect);
                }
            }
            else if (GetDistanceToPlayer(teleportSpot) < teleportRange)
            {
                PluginVerbos($"Is at the distance required for {ZoneID}");
                return true;
            }

            return false;
        }
    }
}
