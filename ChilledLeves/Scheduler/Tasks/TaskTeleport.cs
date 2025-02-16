using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTeleport
    {
        internal static unsafe void Enqueue(uint aetherytID, uint targetTerritoryId)
        {
            P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, targetTerritoryId), "Teleporting to Destination", DConfig);
            P.taskManager.Enqueue(() => IsScreenReady());
        }

        internal static unsafe bool? TeleporttoAethery(uint aetherytID, uint targetTerritoryId)
        {
            if (IsInZone(targetTerritoryId) && PlayerNotBusy())
                return true;

            if (!Svc.Condition[ConditionFlag.Casting] && PlayerNotBusy() && !IsBetweenAreas && !IsInZone(targetTerritoryId))
            {
                if (EzThrottler.Throttle("Teleport Throttle", 7000))
                {
                    PluginLog($"Teleporting to {aetherytID} at {targetTerritoryId}");
                    Telepo.Instance()->Teleport(aetherytID, 0);
                    return false;
                }
            }
            return false;
        }
    }
}
