using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Scheduler.Tasks;
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
using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ChilledLeves.Utilities;

public static unsafe class Utils
{
    #region Plugin/Ecoms stuff

    public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
    public static void PluginVerbos(string message) => Svc.Log.Verbose(message);
    public static void PluginInfo(string message) => Svc.Log.Info(message);
    public static void PluginDebug(string message) => Svc.Log.Debug(message);
    internal static bool GenericThrottle => FrameThrottler.Throttle("AutoRetainerGenericThrottle", 10);
    public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);

    #endregion

    #region Player Information

    internal static unsafe float GetDistanceToPlayer(Vector3 v3) => Vector3.Distance(v3, Player.GameObject->Position);
    internal static unsafe float GetDistanceToPlayer(IGameObject gameObject) => GetDistanceToPlayer(gameObject.Position);
    public static uint GetClassJobId() => Svc.ClientState.LocalPlayer!.ClassJob.RowId;
    public static unsafe int GetLevel(int expArrayIndex = -1)
    {
        if (expArrayIndex == -1) expArrayIndex = Svc.ClientState.LocalPlayer?.ClassJob.Value.ExpArrayIndex ?? 0;
        return UIState.Instance()->PlayerState.ClassJobLevels[expArrayIndex];
    }
    internal static unsafe short GetCurrentLevelFromSheet(Job? job = null)
    {
        PlayerState* playerState = PlayerState.Instance();
        return playerState->ClassJobLevels[Svc.Data.GetExcelSheet<ClassJob>().GetRowOrDefault((uint)(job ?? (Player.Available ? Player.Object.GetJob() : 0)))?.ExpArrayIndex ?? 0];
    }

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

        agent->IsFlagMarkerSet = 0;
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

        var addon = (AtkUnitBase*)ptr;
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
                Job ecomJob = 0;

                ecomJob = EcomJobFinder(leveJob);

                if (!CraftFisherJobs.Contains(leveJob))
                {
                    continue;
                }

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
                leveName = leveName.Replace("<nbsp>", " ");
                leveName = leveName.Replace("<->", "");

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

                // Testing this to see if I can grab upon loading the sheet up to save frames...
                int currentlyHave = GetItemCount(itemID.ToInt());

                uint leveVendor = LeveVendor.FirstOrDefault(pair => pair.Value.Contains(leveNumber)).Key;
                string vendorName = LeveNPCDict[leveVendor].Name;

                // Leve Turnin Vendor, I actually forgot to include this upon loading :catlay:
                uint leveClientId = row.LeveClient.Value.RowId;
                uint turninNpcId = TurninNpcId(leveClientId, JobType: leveJob);

                int priority = 0;

                // Ensure the leveJobType is valid before inserting
                if (!LeveDictionary.ContainsKey(leveNumber))
                {
                    LeveDictionary[leveNumber] = new LeveDataDict
                    {
                        Amount = amount,
                        LeveName = leveName,
                        JobAssignmentType = leveJob,
                        EcomJob = ecomJob,
                        Level = leveLevel,
                        QuestID = questID,
                        ExpReward = expReward,
                        GilReward = gilReward,
                        LeveVendorID = leveVendor,
                        LeveVendorName = vendorName,

                        // Gathering Specific, but can be applied to all
                        Priority = priority,
                    };
                }

                if (!CraftDictionary.ContainsKey(leveNumber) && CraftFisherJobs.Contains(leveJob))
                {
                    CraftDictionary[leveNumber] = new CraftDataDict
                    {
                        // Crafting Specific
                        LeveTurninVendorID = turninNpcId,
                        RepeatAmount = leveRepeat,
                        ItemID = itemID,
                        ItemName = itemName,
                        ItemIcon = itemIcon,
                        TurninAmount = turninAmount,
                        CurrentItemAmount = currentlyHave,
                    };
                }
            }
        }

        for (int i = 0; i < GreyIconList.Count; i++)
        {
            int slot = i + 2;
            int iconId = GreyIconList[i];

            if (Svc.Texture.TryGetFromGameIcon(iconId, out var texture))
            {
                if (!GreyTexture.ContainsKey((uint)slot))
                {
                    GreyTexture.Add((uint)slot, texture);
                }
            }
        }

        for (int i = 0; i < LeveStatus.Count; i++)
        {
            int iconId = LeveStatus[i];

            if (Svc.Texture.TryGetFromGameIcon(iconId, out var texture))
            {
                if (!LeveStatusDict.ContainsKey((uint)i))
                {
                    LeveStatusDict.Add((uint)i, texture);
                }
            }
        }

        foreach (var leveId in C.LevePriority)
        {
            var leve = leveId.Key;
            LeveDictionary[leve].Priority = leveId.Value; //
        }
    }

    public static Job EcomJobFinder(uint leveJob)
    {
        Job ecomJob = 0;

        if (leveJob == 2) // Miner
            ecomJob = (Job)16;
        else if (leveJob == 3) // BTN
            ecomJob = (Job)17;
        else if (leveJob == 4) // FSH
            ecomJob = (Job)18;
        else if (leveJob == 5) // CRP
            ecomJob = (Job)8;
        else if (leveJob == 6) // BSM
            ecomJob = (Job)9;
        else if (leveJob == 7) // ARM
            ecomJob = (Job)10;
        else if (leveJob == 8) // GSM
            ecomJob = (Job)11;
        else if (leveJob == 9) // LTW
            ecomJob = (Job)12;
        else if (leveJob == 10) // WVR
            ecomJob = (Job)13;
        else if (leveJob == 11) // ALC
            ecomJob = (Job)14;
        else if (leveJob == 12) // CUL
            ecomJob = (Job)15;

        return ecomJob;
    }

    public static uint TurninNpcId(uint leveClient, uint JobType = 0)
    {
        uint NPCID = 0;
        if (leveClient == 5)
        {
            // Central Shroud, Audrie
            NPCID = 1001218;
        }
        else if (leveClient == 7)
        {
            // East Shroud, Ayled
            NPCID = 1001219;
        }
        else if (leveClient == 9)
        {
            // New Gridania, Maisenta
            NPCID = 1001276;
        }
        else if (leveClient == 13)
        {
            // South Shroud, Juliembert
            NPCID = 1001220;
        }
        else if (leveClient == 19)
        {
            // Central Shroud, Lanverlais
            NPCID = 1001868;
        }
        else if (leveClient == 113)
        {
            // Eastern La Noscea, Ririphon
            NPCID = 1004345;
        }
        else if (leveClient == 114)
        {
            // Limsa Lominsa Lower Decks, Bango Zango
            NPCID = 1001787;
        }
        else if (leveClient == 115)
        {
            // Lower La Noscea, Zwynwyda
            NPCID = 1004343;
        }
        else if (leveClient == 116)
        {
            // Western La Noscea, Fewon Bulion
            NPCID = 1001790;
        }
        else if (leveClient == 117)
        {
            // Western la Noscea, H'rhanbolo
            NPCID = 1001793;
        }
        else if (leveClient == 118)
        {
            // Ul'dah - Steps of Nald, Roarich
            NPCID = 1004417;
        }
        else if (leveClient == 119)
        {
            // Western Thanalan, Gigiyon
            NPCID = 1003889;
        }
        else if (leveClient == 120)
        {
            // Western Thanalan, Mimina
            NPCID = 1001798;
        }
        else if (leveClient == 121)
        {
            // Easter Thanalan, Frediswitha
            NPCID = 1001801;
        }
        else if (leveClient == 122)
        {
            // Coethras Central Highlands, Vivenne
            NPCID = 1002385;
        }
        else if (leveClient == 123)
        {
            // Coethras Central Highlands, Lanquairt
            NPCID = 1002402;
        }
        else if (leveClient == 124)
        {
            // Mor Dhona, Syele
            NPCID = 1004349;
        }
        else if (leveClient == 141)
        {
            // Unsynwilf, Upper Limsa
            NPCID = 1007060;
        }
        else if (leveClient == 142)
        {
            // Q'molosi, Western La Noscea, Swiftperch
            NPCID = 1007061;
        }
        else if (leveClient == 143)
        {
            // Fupepe, Western La Noscea, Aleport
            NPCID = 1007062;
        }
        else if (leveClient == 144)
        {
            // Daca jinjahl
            NPCID = 1007063;
        }
        else if (leveClient == 145)
        {
            // F'abodji, Eastern La Noscea
            NPCID = 1007064;
        }
        else if (leveClient == 146)
        {
            // Coerthas Central Highlands, SE corner
            NPCID = 1007065;
        }
        else if (leveClient == 147)
        {
            // Louviaune, Coerthas Central Highlands, NW corner
            NPCID = 1007066;
        }
        else if (leveClient == 148)
        {
            NPCID = 1007067;
        }
        else if (IshgardTurnin.Contains(leveClient))
        {
            if (JobType == 4)
                NPCID = 1011210; // Cesteline
            else
                NPCID = 1011209; // Fionnuala
        }
        else if (KuganeTurnin.Contains(leveClient))
        {
            if (JobType == 4)
                NPCID = 1018999; // Geimrael
            else
                NPCID = 1018998; // Chantine
        }
        else if (CrystariumTurnin.Contains(leveClient))
        {
            if (JobType == 4)
                NPCID = 1027849; // Shue-Hann
            else
                NPCID = 1027848; // Moyce
        }
        else if (SharlayanTurnin.Contains(leveClient))
        {
            if (JobType == 4)
                NPCID = 1037265; // Wurtwyb
            else
                NPCID = 1037264; // Ahldiyrn
        }
        else if (TuliyoliTurnin.Contains(leveClient))
        {
            if (JobType == 4)
                NPCID = 1048392;
            else 
                NPCID = 1048391;
        }

        return NPCID;
    }

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
        else
        {
            quest = CanDoLeves(QuestRequired);
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

        foreach (var leve in SelectedLeves)
        {
            var itemId = CraftDictionary[leve].ItemID;
            var currentAmount = GetItemCount((int)itemId);
            var turninAmount = CraftDictionary[leve].TurninAmount;


            if (IsAccepted(leve) && currentAmount >= turninAmount)
            {
                returnLeveId = leve;
                break;
            }
        }

        return returnLeveId;
    }

    public static bool AllCompleted()
    {
        bool allCompleted = true;

        foreach (var leve in SelectedLeves)
        {
            allCompleted &= IsComplete(leve);
        }

        return allCompleted;
    }

    #endregion
}
