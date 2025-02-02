using ChilledLeves.Ui.MainWindow;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using ECommons.Automation.NeoTaskManager;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using static ChilledLeves.Util.Data;

namespace ChilledLeves.Util;

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

    #region Node Visibility | Text

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

    #endregion

    public static unsafe void LeveJobIcons(uint JobType, Vector2 size = default(Vector2))
    {
        if (size == default(Vector2))
        {
            size = new Vector2(20, 20);
        }

        if (GetRow<LeveAssignmentType>(JobType).Value.Icon is { } icon)
        {
            if (Svc.Texture.TryGetFromGameIcon(icon, out var texture))
                ImGui.Image(texture.GetWrapOrEmpty().ImGuiHandle, size);
            else
                ImGui.Dummy(size);
        }
    }

    public static unsafe void ItemIconLookup(uint itemID)
    {
        if (GetRow<Item>(itemID).Value.Icon is { } icon)
        {
            int icon2 = icon;
            if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                ImGui.Image(texture2.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
            else
                ImGui.Dummy(new(20, 20));
        }
    }

    public static unsafe void AllLeves(Vector2 size = default(Vector2))
    {
        if (size == default(Vector2))
        {
            size = new Vector2(20, 20);
        }

        if (GetRow<EventIconPriority>(10).Value.Icon[6] is { } icon)
        {
            int icon2 = (int)icon;
            if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                ImGui.Image(texture2.GetWrapOrEmpty().ImGuiHandle, size);
            else
                ImGui.Dummy(size);
        }
    }

    public static bool IsRowEnabled(uint classId, bool AllEnabled)
    {
        if (AllEnabled)
            return true;
        else
        {
            int spot = 0;

            for (int i = 0; i < CraftingClass.Count; i++)
            {
                if (CraftingClass[i] == classId)
                {
                    spot = i;
                    break;
                }
            }

            return CraftingClassActive[spot];
        }
    }

    public static bool IsLocationEnabled(string location, bool allEnabled)
    {
        if (allEnabled)
            return true;
        else
        {
            int boolLoc = 0;

            for (int i = 0; i < AllLocations.Count; i++)
            {
                if (AllLocations[i] == location)
                {
                    boolLoc = i;
                    break;
                }
            }

            return LocationsActive[boolLoc];
        }
    }

    public static void PopulateDictionary()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();
        var CraftLeveSheet = Svc.Data.GetExcelSheet<CraftLeve>();
        var itemSheet = Svc.Data.GetExcelSheet<Item>();

        if (sheet != null)
        {
            // Checking the Leve sheet (grabbed above) and just making it a lot more shortform
            foreach (var row in sheet)
            {
                // Tells us what the leve number is. This is currently the row that it's *-also-* tied to. 
                // Can't wait for square to bite me in the ass for this later
                uint leveNumber = row.RowId;

                // Checking the Jobtype, this is a very small number 
                uint leveJob = row.LeveAssignmentType.Value.RowId;

                if (!CrafterJobs.Contains(leveJob)) continue;

                // grabbing the job Icon here and putting it into the dictionary.
                // That way I can just pull it from the key itself
                ISharedImmediateTexture? jobIcon = null;
                if (GetRow<LeveAssignmentType>(leveJob).Value.Icon is { } leveJobIcon)
                {
                    int icon2 = leveJobIcon;
                    if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                        jobIcon = texture2;
                }

                // Name of the leve that you're grabbing
                string leveName = row.Name.ToString();

                // Amount to run, this is always 0 upon initializing
                // Mainly there to be an input for users later
                int amount = 0;

                // The questID of the leve. Need this for another sheet but also, might be useful to check progress of other quest...
                uint questID = row.DataId.RowId;
                // Item Id of the items being turned in. This is only really useful for crafters right now
                uint itemID = CraftLeveSheet.GetRow(questID).Item[0].Value.RowId;
                // Item Name itself for the turnin. Ideally this is going to be language based... fingers crossed
                string itemName = itemSheet.GetRow(itemID).Name.ToString();

                // Item Icon, storing this because don't wanna have to draw the sheet every frame and then pull it from the sheet
                ISharedImmediateTexture? itemIcon = null;
                if (GetRow<Item>(itemID).Value.Icon is { } icon)
                {
                    int icon2 = icon;
                    if (Svc.Texture.TryGetFromGameIcon(icon2, out var texture2))
                        itemIcon = texture2;
                }

                // Amount of times you can turn the leve in (aka repeat amount)
                // if it's 0, then it doesn't repeat at all (making it one to show you only do it once
                // 2 = 3 because haha binary numbers
                // +5 is the largescale leves in HW, need to confirm this later if this number is correct (things to do after)
                int leveRepeat = CraftLeveSheet.GetRow(questID).Repeats.ToInt();
                leveRepeat = leveRepeat + 1;

                // Amount of items necessary for turnin
                // Getting this by first getting the amount of possible amount of loops/repeats you do
                int turninAmount = 0;
                for (int x = 0; x < 3; x++)
                {
                    turninAmount = turninAmount + CraftLeveSheet.GetRow(questID).ItemCount[x].ToInt();
                }
                turninAmount = turninAmount * leveRepeat;

                // Defaulting the necessary amount that you need to 0 here
                // Moreso safety precaution than anything
                int necessaryAmount = 0;

                // Starting location that the leve initially starts in 
                // Location location location... always important
                // These are the *-actual-* Zone ID's of the places so, great for teleporting
                uint startingCity = sheet.GetRow(leveNumber).PlaceNameStartZone.Value.RowId;

                // Zone name itself. That way people know exactly where this leve is coming from
                string ZoneName = Svc.Data.GetExcelSheet<PlaceName>().GetRow(startingCity).Name.ToString();

                // Testing this to see if I can grab upon loading the sheet up to save frames...
                int currentlyHave = GetItemCount(itemID.ToInt());

                // Ensure the leveJobType is valid before inserting
                if (!LeveDict.ContainsKey(leveNumber))
                {
                    LeveDict[leveNumber] = new LeveDataDict
                    {
                        JobID = leveJob,
                        JobIcon = jobIcon,
                        LeveName = leveName,
                        Amount = amount,
                        QuestID = questID,
                        RepeatAmount = leveRepeat,
                        StartingCity = startingCity,
                        ZoneName = ZoneName,
                        ItemID = itemID,
                        ItemName = itemName,
                        ItemIcon = itemIcon,
                        TurninAmount = turninAmount,
                        CurrentItemAmount = currentlyHave,
                    };
                }
            }
        }
    }
}
