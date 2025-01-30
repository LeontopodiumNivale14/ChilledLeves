using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using ECommons.Automation.NeoTaskManager;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.MJI;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

namespace ExplorersIcebox.Util;

public static unsafe class Utils
{
    public static bool PluginInstalled(string name)
    {
        return DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    }

    public static unsafe int GetItemCount(int itemID, bool includeHq = true)
        => includeHq ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true) 
        + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
        : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

    public static bool ExecuteTeleport(uint aetheryteId) => UIState.Instance()->Telepo.Teleport(aetheryteId, 0);
    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);
    internal static IGameObject? GetObjectByName(string name) => Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    public static float GetDistanceToPoint(float x, float y, float z) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, new Vector3(x, y, z));
    public static float GetDistanceToPointV(Vector3 targetPoint) => Vector3.Distance(Svc.ClientState.LocalPlayer?.Position ?? Vector3.Zero, targetPoint);
    private static readonly unsafe nint PronounModule = (nint)Framework.Instance()->GetUIModule()->GetPronounModule();
    #pragma warning disable IDE1006 // Naming Styles
    private static readonly unsafe delegate* unmanaged<nint, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<nint, uint, GameObject*>)Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
    #pragma warning restore IDE1006 // Naming Styles
    public static unsafe GameObject* GetGameObjectFromPronounID(uint id) => getGameObjectFromPronounID(PronounModule, id);
    public static bool IsBetweenAreas => (Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51]);
    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", 10);
    public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);
    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public static bool IsInZone(uint zoneID) => Svc.ClientState.TerritoryType == zoneID;

    public static void PluginLog(string message) => ECommons.Logging.PluginLog.Information(message);

    public static bool PlayerNotBusy()
    {
        return Player.Available
               && Player.Object.CastActionId == 0
               && !IsOccupied()
               && !Svc.Condition[ConditionFlag.Jumping]
               && Player.Object.IsTargetable;
    }

    public static (ulong id, Vector3 pos) FindAetheryte(uint id)
    {
        foreach (var obj in GameObjectManager.Instance()->Objects.IndexSorted)
            if (obj.Value != null && obj.Value->ObjectKind == ObjectKind.Aetheryte && obj.Value->BaseId == id)
                return (obj.Value->GetGameObjectId(), *obj.Value->GetPosition());
        return (0, default);
    }

    public static GameObject* LPlayer() => GameObjectManager.Instance()->Objects.IndexSorted[0].Value;

    public static Vector3 PlayerPosition()
    {
        var player = LPlayer();
        return player != null ? player->Position : default;
    }

    public static uint CurrentTerritory() => GameMain.Instance()->CurrentTerritoryTypeId;

    public static bool IsAddonActive(string AddonName) // Used to see if the addon is active/ready to be fired on
    {
        var addon = RaptureAtkUnitManager.Instance()->GetAddonByName(AddonName);
        return addon != null && addon->IsVisible && addon->IsReady;
    }

    public static float GetPlayerRawXPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.X : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.X ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.X;
    }

    public static float GetPlayerRawYPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.Y : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.Y ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.Y;
    }

    public static float GetPlayerRawZPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = Utils.GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.Z : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.Z ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.Z;
    }

    // stuff to get the Node visibility. Moreso to test and see if they have an item unlocked.
    // Thank you Croizat for dealing with me asking dumb questions. I do appreciate it. / having a way this worked in lua

    public static unsafe bool IsNodeVisible(string addonName, params int[] ids)
    {
        var ptr = Svc.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr;
        var node = GetNodeByIDChain(addon->GetRootNode(), ids);
        return node != null && node->IsVisible();
    }

    public static unsafe string GetNodeText(string addonName, params int[] nodeNumbers)
    {

        var ptr = Svc.GameGui.GetAddonByName(addonName, 1);

        var addon = (AtkUnitBase*)ptr;
        var uld = addon->UldManager;

        AtkResNode* node = null;
        var debugString = string.Empty;
        for (var i = 0; i < nodeNumbers.Length; i++)
        {
            var nodeNumber = nodeNumbers[i];

            var count = uld.NodeListCount;

            node = uld.NodeList[nodeNumber];
            debugString += $"[{nodeNumber}]";

            // More nodes to traverse
            if (i < nodeNumbers.Length - 1)
            {
                uld = ((AtkComponentNode*)node)->Component->UldManager;
            }
        }

        if (node->Type == NodeType.Counter)
            return ((AtkCounterNode*)node)->NodeText.ToString();

        var textNode = (AtkTextNode*)node;
        return textNode->NodeText.ExtractText();
    }

    private static unsafe AtkResNode* GetNodeByIDChain(AtkResNode* node, params int[] ids)
    {
        if (node == null || ids.Length <= 0)
            return null;

        if (node->NodeId == ids[0])
        {
            if (ids.Length == 1)
                return node;

            var newList = new List<int>(ids);
            newList.RemoveAt(0);

            var childNode = node->ChildNode;
            if (childNode != null)
                return GetNodeByIDChain(childNode, [.. newList]);

            if ((int)node->Type >= 1000)
            {
                var componentNode = node->GetAsAtkComponentNode();
                var component = componentNode->Component;
                var uldManager = component->UldManager;
                childNode = uldManager.NodeList[0];
                return childNode == null ? null : GetNodeByIDChain(childNode, [.. newList]);
            }

            return null;
        }

        //check siblings
        var sibNode = node->PrevSiblingNode;
        return sibNode != null ? GetNodeByIDChain(sibNode, ids) : null;
    }

    // A way to check to make sure all the values for the sell table is correct
    public static bool CheckIfItemLocked(ushort slotID)
    {
        MJIManager* mjiManager = MJIManager.Instance();

        bool isLocked = mjiManager->IsKeyItemUnlocked(slotID);
        return !isLocked;

        /*
        0 → 6
        8 → 18
        23
        slot | Item | ItemID
        0 → Stone Hatchet (37679)   | 1 → Stone Hammer (37680)
        2 → Islekeep's Shovel (ID)  | 3 → 
        4 → | 5 →
        6 → | 8 → Basic Mammet-Sized Builders Tools (37687)
        9 → | 10 → 
        11 → | 12 → 
        13 → | 14 →
        15 → | 16 →
        17 → | 18 →
        23 →
        */
    }

    public static void UpdateShopCallback()
    {
        PluginLog("---- Starting the Update Callback ----");
        var callback = 0;
        for (int i = 0; i < ListitemIDs.Length; i++)
        {
            var itemID = ListitemIDs[i];
            PluginLog($"Index {i}: ID {itemID}");
            if (IsNodeVisible("MJIPouch", 1, 8, IslandItemDict[itemID].NodeID, 2))
            {
                IslandItemDict[itemID].Callback = callback;
                callback = callback + 1;
                PluginLog($" updated: {callback}");
            }
            else
            {
                IslandItemDict[itemID].Callback = 0;
            }
        }
    }

    // Calulators for Island Sanctuary Routes

    public static void UpdateTableDict()
    {
        foreach (var item in IslandItemDict.Keys.ToList())
        {
            IslandItemDict[item].Amount = GetItemCount(item);
        }
    }

    public static bool DidAmountChange(int lastAmount, int newAmount)
    {
        if (lastAmount == newAmount)
            return false;
        else
            return true;
    }

    public static void FancyCheckmark(bool enabled)
    {
        if (!enabled)
        {
            FontAwesome.Print(ImGuiColors.DalamudRed, FontAwesome.Cross);
        }
        else if (enabled)
        {
            FontAwesome.Print(ImGuiColors.HealerGreen, FontAwesome.Check);
        }
    }
}
