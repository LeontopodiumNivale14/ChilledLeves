using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ECommons.GameFunctions;

namespace ExplorersIcebox.Scheduler.Handers
{
    internal static unsafe class NPCHandlers
    {
        internal static bool? InteractShopNpc()
        {
            var OpenedShopAddonName = "ShopExchangeItem";
            var target = Svc.Targets.Target;
            if (target != default)
            {
                if (IsAddonActive("SelectString") || IsAddonActive("SelectIconString") || IsAddonActive(OpenedShopAddonName))
                    return true;
                unsafe { TargetSystem.Instance()->InteractWithObject(target.Struct(), false); }
            }
            return false;
        }
        internal static bool? TargetShopNpc()
        {
            string NpcName = string.Empty;
            if (Svc.ClientState.TerritoryType == 1055) //Island Sanc
                NpcName = "Enterprising Exporter";

            var target = GetObjectByName(NpcName);
            if (target != null)
            {
                Svc.Targets.Target = target;
                return true;
            }
            return false;
        }
    }
}
