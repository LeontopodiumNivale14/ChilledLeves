using ChilledLeves.Utilities.LeveData;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public static unsafe int GetItemCount(uint itemID, bool includeHq = true)
    {
       return includeHq? InventoryManager.Instance()->GetInventoryItemCount(itemID, true)
            + InventoryManager.Instance()->GetInventoryItemCount(itemID) + InventoryManager.Instance()->GetInventoryItemCount(itemID + 500_000)
            : InventoryManager.Instance()->GetInventoryItemCount(itemID) + InventoryManager.Instance()->GetInventoryItemCount(itemID + 500_000);
    }

    public static int Leve_RequiredAmount(LeveInfo.Material_Turnin materialInfo)
    {
        bool allowMultiTurnin = C.AllowMultiTurnin;
        var turninAmount = materialInfo.TurninAmount;

        if (materialInfo.RepeatAmount > 1 && allowMultiTurnin)
            turninAmount *= materialInfo.RepeatAmount;
            

        return turninAmount;
    }

    public static unsafe void Dismount()
    {
        if (Player.Mounted)
        {
            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
        }
    }

    public static unsafe void MountAction()
    {
        /*
        // bool useMount = C.MountId != 0 && PlayerState.Instance()->IsMountUnlocked(C.MountId);

        if (!Player.IsCasting && !Player.Mounting)
        {
            if (useMount)
            {
                ActionManager.Instance()->UseAction(ActionType.Mount, C.MountId);
                IceLogging.Info($"Attempting to mount: {C.MountName}");
            }
            else
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
                IceLogging.Info($"Resorting to using the mount roulette");
            }
        }
        */
        if (!Player.IsCasting && !Player.Mounting)
        {
            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
        }
    }
}
