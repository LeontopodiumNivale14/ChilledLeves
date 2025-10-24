using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace ChilledLeves.Utilities.Utils
{
    public static unsafe partial class Utils
    {
        // Player Info

        public static bool IsInTerritory(uint TerritoryId) => CurrentTerritory() == TerritoryId;
        public static uint CurrentTerritory() => Svc.ClientState.TerritoryType;

        public static unsafe bool GetItemCount(uint itemID, out int count, bool includeHq = true, bool includeNq = true)
        {
            try
            {
                itemID = itemID >= 1_000_000 ? itemID - 1_000_000 : itemID;
                count = 0;
                if (includeHq)
                    count += InventoryManager.Instance()->GetInventoryItemCount(itemID, true);
                if (includeNq)
                    count += InventoryManager.Instance()->GetInventoryItemCount(itemID, false);
                count += InventoryManager.Instance()->GetInventoryItemCount(itemID + 500_000);
                return true;
            }
            catch
            {
                count = 0;
                return false;
            }
        }

        public static unsafe void SetFlagForNPC(uint teri, float x, float y)
        {
            var terSheet = Svc.Data.GetExcelSheet<TerritoryType>();
            var mapId = terSheet.GetRow(teri).Map.Value.RowId;

            var agent = AgentMap.Instance();

            agent->FlagMarkerCount = 0;
            agent->SetFlagMapMarker(teri, mapId, x, y);
            agent->OpenMapByMapId(mapId, territoryId: teri);
        }

        public static unsafe uint CurrentMap()
        {
            var agent = AgentMap.Instance();
            return agent->CurrentMapId;
        }
    }
}
