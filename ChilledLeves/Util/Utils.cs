using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Internal;
using ECommons.Automation.NeoTaskManager;
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
using Dalamud.Interface.Textures.TextureWraps;
using System.Threading;
using System.Reflection;
using ECommons.DalamudServices.Legacy;

namespace ChilledLeves.Util;

public static unsafe class Utils
{
    #region Ecoms stuff

    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", 10);
    public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);

    #endregion

    #region Plugin Info

    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public static void PluginLog(string message) => ECommons.Logging.PluginLog.Information(message);

    public static bool IsAddonActive(string AddonName) // Used to see if the addon is active/ready to be fired on
    {
        var addon = RaptureAtkUnitManager.Instance()->GetAddonByName(AddonName);
        return addon != null && addon->IsVisible && addon->IsReady;
    }

    public static bool PluginInstalled(string name)
    {
        return DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    }

    #endregion

    #region Player Info
    public static unsafe int GetItemCount(int itemID, bool includeHq = true)
    => includeHq ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true)
    + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
    : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

    public static uint CurrentTerritory() => GameMain.Instance()->CurrentTerritoryTypeId;
    public static bool IsInZone(uint zoneID) => Svc.ClientState.TerritoryType == zoneID;
    public static bool IsBetweenAreas => Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];

    internal static IGameObject? GetObjectByName(string name) => Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(o => o.Name.TextValue.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);

    public static bool PlayerNotBusy()
    {
        return Player.Available
               && Player.Object.CastActionId == 0
               && !IsOccupied()
               && !Svc.Condition[ConditionFlag.Jumping]
               && Player.Object.IsTargetable;
    }

    #endregion

    #region Player Positioning

    // All of this is neeced for the player positioning:
    private static readonly unsafe delegate* unmanaged<nint, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<nint, uint, GameObject*>)Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
    private static readonly unsafe nint PronounModule = (nint)Framework.Instance()->GetUIModule()->GetPronounModule();
    public static unsafe GameObject* GetGameObjectFromPronounID(uint id) => getGameObjectFromPronounID(PronounModule, id);
    public static GameObject* LPlayer() => GameObjectManager.Instance()->Objects.IndexSorted[0].Value;

    // End necessities
    public static Vector3 PlayerPosition()
    {
        var player = LPlayer();
        return player != null ? player->Position : default;
    }
    public static float GetPlayerRawXPos(string character = "")
    {
        if (!character.IsNullOrEmpty())
        {
            unsafe
            {
                if (int.TryParse(character, out var p))
                {
                    var go = GetGameObjectFromPronounID((uint)(p + 42));
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
                    var go = GetGameObjectFromPronounID((uint)(p + 42));
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
                    var go = GetGameObjectFromPronounID((uint)(p + 42));
                    return go != null ? go->Position.Z : -1;
                }
                else return Svc.Objects.Where(x => x.IsTargetable).FirstOrDefault(x => x.Name.ToString().Equals(character))?.Position.Z ?? -1;
            }
        }
        return Svc.ClientState.LocalPlayer!.Position.Z;
    }

    #endregion

    #region Targets, Targeting, and finding oh my

    /// <summary>
    /// Target's the target by it's ID. Moreso for the sake of users/make sure that I have the right thing.
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    internal static bool? TargetByID(IGameObject? gameObject)
    {
        var x = gameObject;
        if (Svc.Targets.Target != null && Svc.Targets.Target.DataId == x.DataId)
            return true;

        if (!IsOccupied())
        {
            if (x != null)
            {
                if (EzThrottler.Throttle($"Throttle Targeting {x.DataId}"))
                {
                    Svc.Targets.SetTarget(x);
                    ECommons.Logging.PluginLog.Information($"Setting the target to {x.DataId}");
                }
            }
        }
        return false;
    }
    internal static bool TryGetObjectByDataId(ulong dataId, out IGameObject? gameObject) => (gameObject = Svc.Objects.OrderBy(GetDistanceToPlayer).FirstOrDefault(x => x.DataId == dataId)) != null;

    #endregion

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

    #region Leve Utilities

    public static unsafe int Allowances => QuestManager.Instance()->NumLeveAllowances;
    public static unsafe TimeSpan NextAllowances => QuestManager.GetNextLeveAllowancesDateTime() - DateTime.Now;

    public static unsafe void LeveJobIcons(uint JobType, Vector2 size = default)
    {
        if (size == default)
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

    public static unsafe void AllLeves(Vector2 size = default)
    {
        if (size == default)
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

    public static void PopulateDictionary()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();
        var CraftLeveSheet = Svc.Data.GetExcelSheet<CraftLeve>();
        var itemSheet = Svc.Data.GetExcelSheet<Item>();
        var assignmentSheet = Svc.Data.GetExcelSheet<LeveAssignmentType>();

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
                string leveType = Svc.Data.GetExcelSheet<LeveAssignmentType>().GetRow(leveJob).Name.ToString();
                foreach (var row2 in assignmentSheet)
                {
                    if (row2.Name != "" && row2.Icon is { } leveJobIcon)
                    {
                        if (Svc.Texture.TryGetFromGameIcon(leveJobIcon, out var texture2))
                        {
                            if (!LeveTypeDict.ContainsKey(row2.RowId))
                            {
                                LeveTypeDict[row2.RowId] = new LeveType
                                {
                                    AssignmentIcon = texture2,
                                    LeveClassType = row2.Name.ToString(),
                                };
                            }
                        }
                    }
                }
                // Name of the leve that you're grabbing
                string leveName = row.Name.ToString();

                uint leveLevel = row.ClassJobLevel;
                int expReward = row.ExpReward.ToInt();
                int gilReward = row.GilReward.ToInt();

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
                // Unless you're limsa, then you're just a pain in the ass to deal with xD
                uint startingCity = sheet.GetRow(leveNumber).PlaceNameStart.Value.RowId;

                // Zone name itself. That way people know exactly where this leve is coming from
                string startingZoneName = Svc.Data.GetExcelSheet<PlaceName>().GetRow(startingCity).Name.ToString();

                // Location of where the leve starts when you pick it up, this is only one for crafters but, this might change for gatherers...

                // Testing this to see if I can grab upon loading the sheet up to save frames...
                int currentlyHave = GetItemCount(itemID.ToInt());

                // Ensure the leveJobType is valid before inserting
                if (!LeveDict.ContainsKey(leveNumber))
                {
                    LeveDict[leveNumber] = new LeveDataDict
                    {
                        JobID = leveJob,
                        Level = leveLevel,
                        LeveName = leveName,
                        ExpReward = expReward,
                        GilReward = gilReward,
                        Amount = amount,
                        QuestID = questID,
                        RepeatAmount = leveRepeat,
                        StartingCity = startingCity,
                        StartingZoneName = startingZoneName,
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

    public static void UpdateItemAmount()
    {
        if (EzThrottler.Throttle("Update Dictionary Amount", 1000))
        {
            foreach (var kdp in LeveDict)
            {
                int itemID = kdp.Value.ItemID.ToInt();
                kdp.Value.CurrentItemAmount = GetItemCount(itemID);
            }
            PluginLog("Updated Inventory");
        }
    }

    #endregion
}
