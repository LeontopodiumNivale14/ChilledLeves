using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskReturn
    {
        internal static unsafe void Enqueue()
        {
            P.taskManager.Enqueue(() => ReturntoBase(), "ReturningToBase");
        }
        internal static unsafe bool? ReturntoBase()
        {
            if (atEntrance && PlayerNotBusy()) return true;

            if (CurrentTerritory() == 1055 && !atEntrance)
            {
                if (!Svc.Condition[ConditionFlag.Casting] && !IsBetweenAreas)
                {
                    ActionManager.Instance()->UseAction(ActionType.GeneralAction, 27);
                }
            }
            return false;
        }
    }
}
