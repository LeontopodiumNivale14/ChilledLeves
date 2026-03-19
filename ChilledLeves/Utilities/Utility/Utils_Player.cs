using FFXIVClientStructs.FFXIV.Client.Game;
namespace ChilledLeves.Utilities;

public static partial class Utils
{
    public static unsafe int GetItemCount(int itemID, bool includeHq = true)
    {
       return includeHq? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true)
            + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
            : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);
    }
}
