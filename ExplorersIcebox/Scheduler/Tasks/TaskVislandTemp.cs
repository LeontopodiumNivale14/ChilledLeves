using Dalamud.Game.ClientState.Conditions;
using ExplorersIcebox.Scheduler.Handers;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskVislandTemp
    {
        internal unsafe static void Enqueue(string VRoute, string Taskname)
        {
            P.taskManager.Enqueue(() => VislandExec(VRoute), Taskname, configuration: DConfig);
        }
        internal unsafe static bool? VislandExec(string VRoute)
        {
            if (P.visland.IsRouteRunning() == false && !Svc.Condition[ConditionFlag.OccupiedInQuestEvent])
            {
                P.visland.StartRoute(VRoute, true);
                return true;
            }

            if (P.visland.IsRouteRunning() || PlayerHandlers.IsMoving()) return false;
            return false;
        }
    }
}
