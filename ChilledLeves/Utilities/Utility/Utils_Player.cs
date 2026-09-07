using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;
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

    // This is the best way to just... grab the flight info.
    // Returns all aether currents being completed/flight directly allowed
    // Credits here: https://github.com/PunishXIV/Questionable/blob/758a8fc1c7d59020ada68b9e6a86ddcc5bfc54eb/Questionable/Functions/GameFunctions.cs#L61 
    // Fucking bless
    // For non-aethercurrent areas (Diadem) need to add a hardcode for it though (as well as island sanc)
    public static unsafe bool CanFly(uint territory = 0)
    {
        Dictionary<uint, uint> _territoryToAetherCurrentCompFlgSet = ExcelHelper.Sheet_TerritoryType
            .Where(x => x.RowId > 0 && x.AetherCurrentCompFlgSet.RowId > 0)
            .ToDictionary(x => x.RowId, x => x.AetherCurrentCompFlgSet.RowId);

        var territoryId = territory != 0 ? territory : Player.Territory.RowId;

        PlayerState* playerState = PlayerState.Instance();
        return playerState != null
            && _territoryToAetherCurrentCompFlgSet.TryGetValue(territoryId, out uint aetherCurrentCompFlgSet)
            && playerState->IsAetherCurrentZoneComplete(aetherCurrentCompFlgSet);
    }

    public static unsafe void Dismount()
    {
        if (Player.Mounted && EzThrottler.Throttle("Dismount Action Applying"))
        {
            ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
        }
    }

    public static unsafe void MountAction()
    {
        const string tag = "Mount Action";

        bool useMount = C.MountId != 0 && PlayerState.Instance()->IsMountUnlocked(C.MountId);

        if (!Player.IsCasting && !Player.Mounting && EzThrottler.Throttle("Using Mount Action"))
        {
            if (useMount)
            {
                ActionManager.Instance()->UseAction(ActionType.Mount, C.MountId);
                IceLogging.Info($"Attempting to mount: {C.MountName}", tag);
            }
            else
            {
                ActionManager.Instance()->UseAction(ActionType.GeneralAction, 9);
                IceLogging.Info($"Resorting to using the mount roulette", tag);
            }
        }
    }

    public unsafe static void TaskClassChange(Job job)
    {
        string tag = "Task: Equip Gearset";

        if (job == Player.Job || !EzThrottler.Throttle("Gearset", 250) || Player.IsBusy)
            return;
        var gearsets = RaptureGearsetModule.Instance();
        foreach (ref var gs in gearsets->Entries)
        {
            if (!RaptureGearsetModule.Instance()->IsValidGearset(gs.Id)) continue;
            if ((Job)gs.ClassJob == job)
            {
                if (gs.Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing))
                {
                    if (GenericHelpers.TryGetAddonMaster<SelectYesno>("SelectYesno", out var select) && select.IsAddonReady)
                    {
                        select.Yes();
                    }
                    else
                    {
                        gearsets->EquipGearset(gs.Id);
                    }
                }

                var result = gearsets->EquipGearset(gs.Id);
                IceLogging.Debug($"Tried to equip gearset {gs.Id} for {job}, result={result}, flags={gs.Flags}", tag);
                return;
            }
        }

        if (EzThrottler.Throttle("No gearsets"))
            IceLogging.Verbose($"Hewwo. We have gotten thiws faw, which means thawt the geawset fow {job.ToString()} doesn't exist. Pwease make owne", tag);
        return;
    }

    public static bool HasStatusId(params uint[] statusIDs)
    {
        if (Player.Object is not IBattleChara battleChara)
            return false;

        return battleChara.StatusList.Any(s => statusIDs.Contains((uint)s.StatusId));
    }

    public static int GetGp() => (int)(Player.Object?.CurrentGp ?? 0);

    public static int MaxGp() => (int)(Player.Object?.MaxGp ?? 0);
}
