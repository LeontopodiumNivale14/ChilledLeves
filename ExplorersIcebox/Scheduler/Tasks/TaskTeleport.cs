using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using ExplorersIcebox.Scheduler.Handers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskTeleport
    {
        internal static unsafe void Enqueue(uint aetherytID, uint targetTerritoryId)
        {
            P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, targetTerritoryId), "Teleporting to Destination", DConfig);
        }

        internal static unsafe bool? TeleporttoAethery(uint aetherytID, uint targetTerritoryId)
        {
            if (IsInZone(targetTerritoryId) && PlayerNotBusy())
                return true;
            if (IsAddonActive("SelectYesno"))
            {
                int YesNo;
                if (C.UseTickets)
                    YesNo = 0;
                else
                    YesNo = 1;
                if (EzThrottler.Throttle("Callback Throttle", 500))
                    GenericHandlers.FireCallback("SelectYesno", true, YesNo);
            }

            if (!Svc.Condition[ConditionFlag.Casting] && PlayerNotBusy() && !IsBetweenAreas && !IsInZone(targetTerritoryId))
            {
                if (EzThrottler.Throttle("Teleport Throttle", 6500))
                {
                    Telepo.Instance()->Teleport(aetherytID, 0);
                    return false;
                }
            }
            return false;
        }
    }
}
