using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskMountUp
    {
        public static void Enqueue()
        {
            P.taskManager.Enqueue(() => SchedulerMain.CurrentProcess = "Mounting up");
            P.taskManager.Enqueue(() => MountUp());
        }
        // Mounting up on... well a mount. 
        internal unsafe static bool? MountUp()
        {
            if (Svc.Condition[ConditionFlag.Mounted] && PlayerNotBusy()) return true;

            if (!Svc.Condition[ConditionFlag.Casting] && !Svc.Condition[ConditionFlag.Unknown57] && PlayerNotBusy())
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 24);
                PluginLog("Attempting to mount up");
            }

            return false;
        }
    }
}
