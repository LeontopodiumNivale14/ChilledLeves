using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using ECommons.Automation.NeoTaskManager;
using ECommons.DalamudServices.Legacy;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Reflection;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.Interop;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities.OldUtils;

public static unsafe class Old_Utils
{
    #region Plugin/Ecoms stuff

    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public static void PluginVerbos(string message) => ECommons.Logging.PluginLog.Verbose(message);
    public static void PluginInfo(string message) => ECommons.Logging.PluginLog.Information(message);
    public static void PluginDebug(string message) => ECommons.Logging.PluginLog.Debug(message);
    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", 10);
    public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);

    #endregion

    public static bool ContainsIgnoreSpacesAndCase(string source, string search)
    {
        var normalizedSource = source.Replace(" ", "").ToLowerInvariant();
        var normalizedSearch = search.Replace(" ", "").ToLowerInvariant();
        return normalizedSource.Contains(normalizedSearch);
    }

    #region Player Information

    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);
    public static uint GetClassJobId() => Player.ClassJob.RowId;

    public static unsafe float GetJobExp(uint classjob) => PlayerState.Instance()->ClassJobExperience[GetRow<ClassJob>(classjob)?.ExpArrayIndex ?? 0];
    public static bool IsInZone(uint zoneID) => Svc.ClientState.TerritoryType == zoneID;
    public static uint CurrentTerritory() => GameMain.Instance()->CurrentTerritoryTypeId;
    public static bool IsBetweenAreas => Svc.Condition[ConditionFlag.BetweenAreas] || Svc.Condition[ConditionFlag.BetweenAreas51];

    public static bool PlayerNotBusy()
    {
        return Player.Available
               && Player.Object.CastActionId == 0
               && !IsOccupied()
               && !Svc.Condition[ConditionFlag.Jumping]
               && !Svc.Condition[ConditionFlag.Jumping61]
               && Player.Object.IsTargetable
               && !Player.IsAnimationLocked;
    }

    public static unsafe int GetItemCount(int itemID, bool includeHq = true)
        => includeHq ? InventoryManager.Instance()->GetInventoryItemCount((uint)itemID, true)
        + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000)
        : InventoryManager.Instance()->GetInventoryItemCount((uint)itemID) + InventoryManager.Instance()->GetInventoryItemCount((uint)itemID + 500_000);

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


    #endregion

    #region Target Information

    internal static bool? TargetgameObject(IGameObject? gameObject)
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
    internal static unsafe void InteractWithObject(IGameObject? gameObject)
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
            Svc.Log.Info($"InteractWithObject: Exception: {ex}");
        }
    }

    #endregion

    #region Node Visibility / Text

    public static unsafe bool IsNodeVisible(string addonName, params int[] ids)
    {
        var ptr = Svc.GameGui.GetAddonByName(addonName, 1);
        if (ptr == nint.Zero)
            return false;

        var addon = (AtkUnitBase*)ptr.Address;
        var node = GetNodeByIDChain(addon->GetRootNode(), ids);
        return node != null && node->IsVisible();
    }

    /// <summary>
    /// AddonName is the name of the Window itself
    /// nodeNumbers are IN the Node List section, and they're the first [#] in the brackets
    /// Need to go down through each Nodelist Tree...
    /// </summary>
    /// <param name="addonName"></param>
    /// <param name="nodeNumbers"></param>
    /// <returns></returns>
    public static unsafe string GetNodeText(string addonName, params int[] nodeNumbers)
    {

        var ptr = Svc.GameGui.GetAddonByName(addonName, 1);

        var addon = (AtkUnitBase*)ptr.Address;
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
        return textNode->NodeText.GetText();
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
    public static bool IsAddonActive(string AddonName) // Used to see if the addon is active/ready to be fired on
    {
        var addon = RaptureAtkUnitManager.Instance()->GetAddonByName(AddonName);
        return addon != null && addon->IsVisible && addon->IsReady;
    }


    public static int GetCallback(string questName)
    {
        int callback = 0;

        for (int i = 1; i < 18; i++)
        {
            string CurrentText = GetNodeText("SelectIconString", 2, i, 4);
            if (CurrentText == questName)
            {
                callback = i - 1;
                PluginVerbos($"Callback number is: {callback}");
                break;
            }
        }

        return callback;
    }

    #endregion

    #region Leve Utilities

    public static string NPCName(uint NpcID)
    {
        var NPCSheet = Svc.Data.GetExcelSheet<ENpcResident>();
        return NPCSheet.GetRow(NpcID).Singular.ToString();
    }

    public static string ZoneName(uint ZoneID)
    {
        var TerritorySheet = Svc.Data.GetExcelSheet<TerritoryType>();
        return TerritorySheet.GetRow(ZoneID)!.PlaceName.Value.Name.ToString() ?? "n/a";
    }

    #endregion

    #region Leve Information

    public static unsafe int Allowances => QuestManager.Instance()->NumLeveAllowances;
    public static unsafe TimeSpan NextAllowances => QuestManager.GetNextLeveAllowancesDateTime() - DateTime.Now;

    public static unsafe LeveWork* GetLeveWork(uint leveId)
    {
        var leveQuests = QuestManager.Instance()->LeveQuests;

        for (var i = 0; i < leveQuests.Length; i++)
        {
            if (leveQuests[i].LeveId == (ushort)leveId)
            {
                return leveQuests.GetPointer(i);
            }
        }

        return null;
    }

    public static unsafe bool IsStarted(uint leveId)
    {
        var leveWork = GetLeveWork((ushort)leveId);
        if (leveWork == null)
            return false;

        return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
    }

    public static unsafe bool IsComplete(uint leveID)
        => QuestManager.Instance()->IsLevequestComplete((ushort)leveID);

    public static unsafe bool IsMSQComplete(uint questID)
        => QuestManager.IsQuestComplete((ushort)questID);

    public static bool IsAccepted(uint leveID)
        => GetActiveLeveIds().Any(id => id == (ushort)leveID);

    public static unsafe IEnumerable<ushort> GetActiveLeveIds()
    {
        var leveIds = new HashSet<ushort>();

        foreach (ref var entry in QuestManager.Instance()->LeveQuests)
        {
            if (entry.LeveId != 0)
                leveIds.Add(entry.LeveId);
        }

        return leveIds;
    }

    public static int GetNumAcceptedLeveQuests() => GetActiveLeveIds().Count();

    public static int CallbackSelect(int level)
    {
        if (level < 60)
            return 2;
        else
            return 1;
    }

    public static bool BothQuest()
    {
        bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
        bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
        bool GridaniaNPC = IsMSQComplete(65665); // Spirithold Broken

        if (LimsaLeveNPC)
            return LevesofSwiftperch;
        else if (UlDahLeveNPC)
            return LevesofHorizon;
        else if (GridaniaNPC)
            return LevesofBentbranch;
        else
            return false;
    }

    public static bool HeistLevesUnlocked()
    {
        return BothQuest() && (IsMSQComplete(SimplyTheHestGridania) || IsMSQComplete(SimplyTheHestLimsa) || IsMSQComplete(SimplyTheHestUldah));
    }

    public static bool CanDoLeves(uint QuestRequired)
    {
        return (BothQuest() && IsMSQComplete(QuestRequired));
    }

    public static bool QuestChecker(uint QuestRequired)
    {
        bool quest = false;

        if (QuestRequired == 0)
        {
            quest = BothQuest();
        }
        else if (QuestRequired == 1)
        {
            quest = HeistLevesUnlocked();
        }

        return quest;
    }

    public static void FancyCheckmark(bool enabled)
    {
        float columnWidth = ImGui.GetColumnWidth();  // Get column width
        float rowHeight = ImGui.GetTextLineHeightWithSpacing();  // Get row height

        Vector2 iconSize = ImGui.CalcTextSize($"{FontAwesome.Cross}"); // Get icon size
        float iconWidth = iconSize.X;
        float iconHeight = iconSize.Y;

        float cursorX = ImGui.GetCursorPosX() + (columnWidth - iconWidth) * 0.5f;
        float cursorY = ImGui.GetCursorPosY() + (rowHeight - iconHeight) * 0.5f;

        cursorX = Math.Max(cursorX, ImGui.GetCursorPosX()); // Prevent negative padding
        cursorY = Math.Max(cursorY, ImGui.GetCursorPosY());

        ImGui.SetCursorPos(new Vector2(cursorX, cursorY));

        if (!enabled)
        {
            FontAwesome.Print(ImGuiColors.DalamudRed, FontAwesome.Cross);
        }
        else if (enabled)
        {
            FontAwesome.Print(ImGuiColors.HealerGreen, FontAwesome.Check);
        }
    }

    public static uint HasAcceptedLeve()
    {
        uint returnLeveId = 0;

        return returnLeveId;
    }

    public static bool AllCompleted()
    {
        bool allCompleted = true;
        return allCompleted;
    }

    #endregion
}
