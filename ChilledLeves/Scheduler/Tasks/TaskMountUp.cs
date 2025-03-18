using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;
using ECommons.GameHelpers;

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
            if (Player.Mounted && PlayerNotBusy()) return true;

            if (!Svc.Condition[ConditionFlag.Casting] && !Player.Mounting && PlayerNotBusy())
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
                PluginDebug("Attempting to mount up");
            }



            return false;
        }
    }
}
