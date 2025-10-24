using ECommons.ExcelServices;
using ECommons.GameHelpers;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class Task_JobChange
    {
        private static void Enqueue(uint jobId)
        {
            P.taskManager.Enqueue(() => ChangeClass(jobId));
        }

        private static bool? ChangeClass(uint jobId)
        {
            return true;
        }
    }
}
