using ChilledLeves.Enums;
using ChilledLeves.Scheduler.Tasks;

namespace ChilledLeves.Scheduler
{
    internal static unsafe class SchedulerMain
    {
        internal static bool AreWeTicking;
        internal static bool EnableTicking
        {
            get => AreWeTicking;
            private set => AreWeTicking = value;
        }
        internal static bool EnablePlugin()
        {
            EnableTicking = true;
            return true;
        }
        internal static bool DisablePlugin()
        {
            P.navmesh.Stop();
            P.taskManager.Tasks.Clear();
            P.taskManager.Abort();
           
            return true;
        }

        internal static void Tick()
        {
            if (P.taskManager.NumQueuedTasks == 0 && !Leve_Helper.IsIdle)
            {
                switch (Leve_Helper.State)
                {
                    case Leve_State.CheckLeves: Task_CheckLeves.Enqueue(); break;
                    // case Leve_State.Travel: Task_Travel.
                    default: DisablePlugin(); break;
                }
            }
        }
    }
}
