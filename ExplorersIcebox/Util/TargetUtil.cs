using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices.Legacy;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace ExplorersIcebox.Util
{
    internal class TargetUtil
    {
        internal static bool TryGetObjectByDataId(ulong dataId, out IGameObject? gameObject) => (gameObject = Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(x => x.DataId == dataId)) != null;
        internal static bool TryGetObjectByObjectId(ulong ObjectID, out IGameObject? gameObject) => (gameObject = Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(x => x.GameObjectId == ObjectID)) != null;
        
        internal static unsafe void InteractWithObject(IGameObject? gameObject)
        {
            try
            {
                if (gameObject == null || !gameObject.IsTargetable)
                    return;
                var gameObjectPointer = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)gameObject.Address;
                TargetSystem.Instance()->InteractWithObject(gameObjectPointer, false);
            }
            catch (Exception ex)
            {
                Svc.Log.Info($"InteractWithObject: Exception: {ex}");
            }
        }
        
        internal static bool? TargetByID(IGameObject? gameObject)
        {
            if (!IsOccupied()) 
            {
                var x = gameObject;
                if (x != null)
                {
                    Svc.Targets.SetTarget(x);
                    PluginLog($"Setting the target to {x.DataId}");
                    return true;
                }
            }
            return false;
        }
    }
}
