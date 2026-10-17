using ChilledLeves.Enums;
using ChilledLeves.Scheduler.Tasks;
using ChilledLeves.Utilities.LogInfo;

namespace ChilledLeves.Scheduler
{
    internal static unsafe class SchedulerMain
    {
        internal static bool AreWeTicking => !Leve_Helper.IsIdle;

        internal static void DisablePlugin()
        {
            IceLogging.Verbose($"We were told to stop. So we stopping. Previous state: {Leve_Helper.State}", "Schedular: Disable Plugin");

            Leve_Helper.State = LeveState.Idle;

            P.navmesh.Stop();
            P.taskManager.Tasks.Clear();
            P.navTask.Tasks.Clear();
            P.taskManager.Abort();
            P.navTask.Abort();
        }

        internal static void Tick()
        {
            if (P.taskManager.NumQueuedTasks == 0 && !Leve_Helper.IsIdle)
            {
                Action enqueue = Leve_Helper.State switch
                {
                    LeveState.CheckLeves => Task_CheckLeves.Enqueue,

                    LeveState.Grab_StandardLeve => Task_GrabLeve.Enqueue_Standard,
                    LeveState.Grab_ARRLeve => Task_GrabLeve.Enqueue_ARR,

                    LeveState.GatheringLeve_Start => Task_GatherLeve.Travel_Enqueue,
                    LeveState.GatherLeve_Execute => Task_GatherLeve.Gather_Check,

                    LeveState.Turnin_Leve => Task_Turnin.Enqueue,
                    _ => DisablePlugin,
                };

                enqueue();
            }
        }
    }
}
