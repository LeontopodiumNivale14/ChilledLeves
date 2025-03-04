using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTurninMulti
    {
        internal static unsafe void Enqueue(string QuestName, uint leveID)
        {
            P.taskManager.Enqueue(() => TurninMulti());
            P.taskManager.Enqueue(() => !IsAccepted(leveID));
            P.taskManager.Enqueue(() => PlayerNotBusy());
        }

        internal static unsafe bool? TurninMulti()
        {
            return true;
        }
    }
}
