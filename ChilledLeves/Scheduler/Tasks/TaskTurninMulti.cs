using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTurninMulti
    {
        internal static unsafe void Enqueue(uint zoneId)
        {
            foreach (var leve in C.workList)
            {
                var leveId = leve.LeveID;
                var turninNPC = CraftDictionary[leveId].LeveTurninVendorID;
                var leveName = LeveDictionary[leveId].LeveName;
                var npcZone = LeveNPCDict[turninNPC].ZoneID;

                if (npcZone == zoneId && IsAccepted(leveId))
                {
                    P.taskManager.Enqueue(() => PluginInfo($"Starting the turnin process for LeveID: {leveId} | Levename: {leveName}"));
                    TaskTarget.Enqueue(turninNPC);
                    TaskInteract.Enqueue(turninNPC);
                    TaskTurnin.Enqueue(leveName, leveId);
                    TaskUpdateWorkList.Enqueue(leveId);
                    if (C.IncreaseDelay)
                        P.taskManager.EnqueueDelay(1000);
                }
            }
        }
    }
}
