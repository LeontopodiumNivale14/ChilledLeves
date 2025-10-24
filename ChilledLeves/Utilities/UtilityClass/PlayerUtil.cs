using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices.Legacy;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace ChilledLeves.Utilities.Utils
{
    public static partial class Utils
    {
        // Player Utility (Mainly World Interaction/NPC Interaction Stuff)

        public static void TargetgameObject(IGameObject? gameObject)
        {
            var x = gameObject;
            var currentTarget = Svc.Targets.Target;
            if (currentTarget != null && currentTarget.BaseId == x.BaseId)
                return;

            if (!GenericHelpers.IsOccupied())
            {
                if (x != null)
                {
                    if (EzThrottler.Throttle($"Throttle targeting: {x.BaseId}"))
                    {
                        PluginInfo($"Attempting to set the target to: {x.BaseId} | {x.Name}", "[Target Game Object]");
                        Svc.Targets.SetTarget(x);
                    }
                }
            }
        }
        public static unsafe void InteractWithObject(IGameObject? gameObject)
        {
            try
            {
                if (gameObject == null || !gameObject.IsTargetable)
                    return;
                var gameObjectPointer = (GameObject*)gameObject.Address;
                TargetSystem.Instance()->InteractWithObject(gameObjectPointer, false);
            }
            catch (Exception ex)
            {
                PluginError($"InteractWithObject: Exception: {ex}");
            }
        }
    }
}
