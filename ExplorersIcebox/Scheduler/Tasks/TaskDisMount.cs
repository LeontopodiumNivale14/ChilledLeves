using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace ExplorersIcebox.Scheduler.Tasks
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

            if (CurrentTerritory() == 1055)
            {
                if (Svc.Condition[ConditionFlag.Mounted])
                {
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 24);
                    PluginLog("Attempting to mount up");
                }
            }
            return false;
        }
    }
}
