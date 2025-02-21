using Dalamud.Game.ClientState.Conditions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskDisMount
    {
        public static void Enqueue()
        {
            P.taskManager.Enqueue(() => DisMount());
        }
        internal unsafe static bool? DisMount()
        {
            if (!Svc.Condition[ConditionFlag.Mounted] && PlayerNotBusy()) return true;

            if (Svc.Condition[ConditionFlag.Mounted])
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 24);
                if (EzThrottler.Throttle("Dismount Attempt Log Spam", 2000))
                    PluginLog("Attempting to dismount ya mount");
            }
            return false;
        }
    }
}
