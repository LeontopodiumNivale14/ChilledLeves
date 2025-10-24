﻿using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ChilledLeves.Scheduler.Tasks.OldTask
{
    internal static class TaskTeleport
    {
        internal static unsafe void Enqueue(uint aetherytID, uint targetTerritoryId)
        {
            if (targetTerritoryId == 128)
            {
                P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, 129), "Teleporting to Limsa Lower");
                TaskUseAethernet.Enqueue(128);
            }
            else if (targetTerritoryId == 129)
            {
                P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, 129), "Teleporting to Limsa Lower");
                if (IsInZone(128))
                    TaskUseAethernet.Enqueue(129);
            }
            else if (targetTerritoryId == 819 || targetTerritoryId == 130)
            {
                P.taskManager.Enqueue(() => PluginVerbos($"Target Territory is: {targetTerritoryId}"));
                P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, targetTerritoryId));
                P.taskManager.Enqueue(() => PluginVerbos($"Using the Aethernet"));
                TaskUseAethernet.Enqueue(targetTerritoryId);
            }
            else
            {
                P.taskManager.Enqueue(() => PluginVerbos($"No special aethernet necessary, just teleporting to: {targetTerritoryId}"));
                P.taskManager.Enqueue(() => TeleporttoAethery(aetherytID, targetTerritoryId));
            }
        }

        internal static unsafe bool? TeleporttoAethery(uint aetherytID, uint targetTerritoryId)
        {
            if (IsScreenReady())
            {
                if (IsInZone(targetTerritoryId))
                    return true;
                else if (targetTerritoryId == 129 && IsInZone(128))
                    return true;
            }

            if (!Svc.Condition[ConditionFlag.Casting] && PlayerNotBusy() && !IsBetweenAreas && !IsInZone(targetTerritoryId))
            {
                if (EzThrottler.Throttle("Teleport Throttle", 1000))
                {
                    PluginVerbos($"Teleporting to {aetherytID} at {targetTerritoryId}");
                    Telepo.Instance()->Teleport(aetherytID, 0);
                    return false;
                }
            }
            return false;
        }
    }
}
