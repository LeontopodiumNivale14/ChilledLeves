using ECommons.GameFunctions;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Serilog;

namespace ExplorersIcebox.Scheduler.Tasks
{
    internal static class TaskSellTo
    {
        public static void Enqueue()
        {
            P.taskManager.Enqueue(() => TargetNpc(), "Targeting the NPC");
            P.taskManager.Enqueue(() => TargetInteract(), "Interacting w/ target");
        }
        internal static bool? TargetNpc()
        {
            string NpcName = string.Empty;
            if (Svc.ClientState.TerritoryType == 1055) //Idyllshire
                NpcName = "Enterprising Exporter";
            Log.Debug("TargetNpc" + NpcName);

            var target = GetObjectByName(NpcName);
            if (target != null)
            {
                if (EzThrottler.Throttle("TargetNpc", 20))
                    Svc.Targets.Target = target;
                return true;
            }
            return false;
        }
        internal unsafe static bool? TargetInteract()
        {
            Log.Debug("TargetInteract");
            var target = Svc.Targets.Target;
            if (target != null)
            {
                if (IsAddonActive("SelectString") || IsAddonActive("SelectIconString") || IsAddonActive("ShopExchangeItem"))
                    return true;

                if (EzThrottler.Throttle("TargetInteract", 100))
                    TargetSystem.Instance()->InteractWithObject(target.Struct(), false);


                return false;
            }
            return false;
        }
    }
}
