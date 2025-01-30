using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ExplorersIcebox.Scheduler.Handers;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskMoveTo
    {
        internal unsafe static void Enqueue(Vector3 targetPosition, string destination, bool fly, float toleranceDistance = 3f)
        {
            P.taskManager.Enqueue(() => MoveTo(targetPosition, fly, toleranceDistance), destination, configuration: DConfig);
        }
        internal unsafe static bool? MoveTo(Vector3 targetPosition, bool fly, float toleranceDistance = 3f)
        {
            if (targetPosition.Distance(Player.GameObject->Position) <= toleranceDistance)
            {
                P.navmesh.Stop();
                return true;
            }
            if (P.navmesh.PathfindInProgress() || P.navmesh.IsRunning() || PlayerHandlers.IsMoving()) return false;

            P.navmesh.PathfindAndMoveTo(targetPosition, fly);
            P.navmesh.SetAlignCamera(false);
            return false;
        }
        private static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 1000, abortOnTimeout: false);
    }
}
