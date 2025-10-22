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
            Job ecomJob = EcomJobFinder(C.ClassJobType);

            if (Player.Job == 
        }
    }
}
