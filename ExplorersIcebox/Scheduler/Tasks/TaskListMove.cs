using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskListMove
    {
        internal static void Enqueue(List<Vector3> list, bool fly)
        {
            P.taskManager.Enqueue(() => displayCurrentTask = "Testing Route???");
            P.taskManager.Enqueue(() => P.navmesh.MoveTo(new List<Vector3>(list), false), "Pathing to target");
            P.taskManager.Enqueue(() => IsVnavRunning(), "Nav is running");
            P.taskManager.Enqueue(() => !IsVnavRunning(), "Waiting for nav to not run");
            P.taskManager.Enqueue(() => displayCurrentTask = "");
        }

        internal unsafe static bool? IsVnavRunning()
        {
            if (P.navmesh.IsRunning())
            {
                P.navmesh.SetAlignCamera(false);
                return true;
            }

            if (P.navmesh.PathfindInProgress() || !P.navmesh.IsRunning())
                return false;

            return false;
        }
    }
}
