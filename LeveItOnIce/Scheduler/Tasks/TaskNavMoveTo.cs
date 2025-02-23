using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ChilledLeves.Scheduler.Handers;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskMoveTo
    {
        internal unsafe static void Enqueue(Vector3 targetPosition, string destination, bool fly, float toleranceDistance = 3f)
        {
            if (targetPosition == new Vector3(27.59f, -8.00f, 108.01f) && IsInZone(132))
            {
                P.taskManager.Enqueue(() => MoveTo(new Vector3(61.19f, -8.26f, 103.20f), false, 1), DConfig);
            }

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
