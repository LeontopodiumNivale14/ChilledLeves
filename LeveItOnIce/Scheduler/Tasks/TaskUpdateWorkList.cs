using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskUpdateWorkList
    {
        internal unsafe static void Enqueue(uint leveID)
        {
            P.taskManager.Enqueue(() => UpdateWorkList(leveID));
        }

        internal unsafe static bool? UpdateWorkList(uint leveID)
        {
            #pragma warning disable CS8600
            LeveEntry foundEntry = C.workList.FirstOrDefault(entry => entry.LeveID == leveID);

            if (foundEntry != null)
            {
                foundEntry.InputValue = foundEntry.InputValue - 1;
                PluginLog($"Updated {leveID} to now have {foundEntry.InputValue}");
            }

            return true;
        }
    }
}
