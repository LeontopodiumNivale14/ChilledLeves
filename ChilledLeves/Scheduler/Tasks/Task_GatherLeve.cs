using ChilledLeves.Config_Files;
using ChilledLeves.Enums;
using ChilledLeves.Resources;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.GatheringHelper;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using Dalamud.Game.ClientState.Conditions;
using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_GatherLeve
    {
        // Some things to note so I don't forget
        // - When a leve has been completed/failed (assuming on the failed) = Occupied33
        // - When a leve is currently active = BountByDuty

        public static void Travel_Enqueue()
        {
            var currentLeve = Leve_Helper.LeveToGrab;
            string tag = "Task: Starting Gather Leve";

            IceLogging.Verbose("Checking to see if we need to travel to the leve area", tag);
            if (RouteLoader.Leve_Routes.TryGetValue(currentLeve, out var routeInfo))
            {
                if (routeInfo.NodeInfo.Count > 0)
                {
                    bool correctTerritory = Player.Territory.RowId == routeInfo.TerritoryId;
                    if (correctTerritory)
                    {
                        foreach (var node in routeInfo.NodeInfo)
                        {
                            if (Player.DistanceTo(node.Position) < 5)
                            {
                                IceLogging.Verbose("We're already close enough to a gathering node already! Going to start it next", tag);
                                P.taskManager.Enqueue(() => Initiate_GatheringLeve(), "Initiating Gathering Leve");

                                break;
                            }
                        }

                        IceLogging.Verbose("We're not close enough to a node, so we're just going to queue up the traveling to it", tag);
                    }

                    IceLogging.Verbose("Queueing up travel system now", tag);
                    P.taskManager.Enqueue(() => Initial_GatherTravel(routeInfo), "Traveling for gathering leve");
                    P.taskManager.Enqueue(() => Initiate_GatheringLeve(), "Initiating Gathering Leve");
                }
                else
                {
                    IceLogging.Error("We seem to be just... missing nodes on this route. Please report this", tag);
                    IceLogging.Error("If you would like to do other leves, please remove this from your list", tag);
                    IceLogging.Error($"ID: [{currentLeve}]", tag);
                    SchedulerMain.DisablePlugin();
                }
            }
            else
            {
                IceLogging.Error("No route information was found upon storage. (Neither an internal build one or external)", tag);
                IceLogging.Error("If you have a custom route installed, please make sure that it is loaded", tag);
                IceLogging.Error($"Otherwise, report this to the dev (hi). {currentLeve}", tag);
                SchedulerMain.DisablePlugin();
            }
        }

        public static void Gather_Check()
        {
            /* This is going to be where the list of task for the leve is while mid mission.
             * Thinking it'll go
             * - Check to see if we're near an active node
             *   - If yes, interact -> gather like normal
             *   - If no, do node finder process
             * - [Node Finder Process]
             * - If it's the mission where it's gather at 8/12 nodes (pita) [Search / Search & Procure] [Execute, except 4 nodes here]
             *     - Log all currently active nodes that are within range
             *     - Mark which ones are not active and throw them into an ignore list to not check again
             *     - Find the closest one to us
             *     - Pathfind to that specific node
             *     - Interact like normal
             * - If procure [Just need to gather 45 items]
             *     - Find active node -> pathfind and interact with it (simple af)
            */

            var leve = Leve_Helper.LeveToGrab;
            var leveInfo = LeveInfo.Leve_SheetInfo[leve];
            var routeInfo = RouteLoader.GetRoute(leve);

            P.taskManager.Enqueue(() => CheckRules(leveInfo, routeInfo), "Checking rules for node filtering");
        }

        private static GatheringNode goalPosition = null;

        private static bool Initial_GatherTravel(GatheringRoute routeInfo)
        {
            string tag = "Gather: Start Travel";

            var navtask = P.navTask;
            bool correctTerritory = Player.Territory.RowId == routeInfo.TerritoryId;

            if (!navtask.IsBusy)
            { 
                if (correctTerritory)
                {
                    var closestNode = routeInfo.NodeInfo.OrderBy(x => Player.DistanceTo(x.Position)).FirstOrDefault();
                    if (Player.DistanceTo(closestNode.Position) < 5)
                    {
                        IceLogging.Verbose($"We're close to the gathering node. So we're going to continue on", tag);
                        IceLogging.Verbose($"Distance to node: {Player.DistanceTo(closestNode.Position):N2}", tag);
                        return true;
                    }
                    else
                    {
                        IceLogging.Verbose($"We need to move closer to a node. Currently... our closest node is {Player.DistanceTo(closestNode.Position):N2}", tag);
                        Task_Navmesh.Queue_GatherTravel(routeInfo);
                    }
                }
                else
                {
                    IceLogging.Verbose("We're not in the area, but we still need to travel. So gonna queue up navmesh", tag);
                    Task_Navmesh.Queue_GatherTravel(routeInfo);
                }
            }
            else
            {
                if (EzThrottler.Throttle("Travel: Gather", 1000))
                    IceLogging.Verbose("We are currently busy nav-tasking. Waiting for us to be done. . . ", tag);

                if (correctTerritory)
                {
                    var closestNode = routeInfo.NodeInfo.OrderBy(x => Player.DistanceTo(x.Position)).FirstOrDefault();
                    
                }
            }

            return false;
        }

        private static unsafe bool Initiate_GatheringLeve()
        {
            string tag = "Gather: Initiate";

            var leve = Leve_Helper.LeveToGrab;

            if (Svc.Condition[ConditionFlag.BoundByDuty])
            {
                IceLogging.Verbose("We've successfully launched into a gathering mission. Time for that bread!", tag);
                CheckedNodes.Clear();
                P.taskManager.EnqueueDelay(2000);
                Leve_Helper.State = LeveState.GatherLeve_Execute;
                return true;
            }
            else if (GenericHelpers.TryGetAddonMaster<GuildLeveDifficulty>(out var glDifficulty) && glDifficulty.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting level + Yes"))
                {
                    glDifficulty.SliderLevel(glDifficulty, 0);
                    glDifficulty.Yes();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<SelectYesno>(out var yesNoAddon) && yesNoAddon.IsAddonReady)
            {
                if (EzThrottler.Throttle("selecting yes to continue"))
                {
                    IceLogging.Verbose("Ignoring the warning of returning mid mission... we don't care", tag);
                    yesNoAddon.Yes();
                }
            }
            else if (GenericHelpers.TryGetAddonMaster<JournalDetail>(out var journalDetail) && journalDetail.IsAddonReady)
            {
                var job = LeveInfo.Leve_SheetInfo[leve].Job;
                if (Player.Job != job)
                {
                    if (EzThrottler.Throttle("Swap jobs", 2000))
                    {
                        IceLogging.Verbose("We need to swap jobs to even be able to do this, so that's what we're going to do. *-I hope you have one unlocked-*", tag);
                        Utils.TaskClassChange(job);
                    }

                    return false;
                }

                if (journalDetail.CanInitiate)
                {
                    if (EzThrottler.Throttle("Initiating quest", 2000))
                    {
                        IceLogging.Verbose("We can initiate this quest, so we're going to do so.", tag);
                        journalDetail.Initiate();
                    }
                }
            }
            else
            {
                if (EzThrottler.Throttle("Opening Journal", 5000))
                {
                    IceLogging.Verbose("We are needing to open the journal to the right leve, so going to do so", tag);
                    AgentQuestJournal.Instance()->OpenForQuest(leve, 2, keepOpen: true);
                }

            }

            return false;
        }

        public static List<uint> CheckedNodes = new();

        private static bool CheckRules(LeveInfo.Leve_SheetData leveInfo, GatheringRoute routeInfo)
        {
            string tag = "Gathering: Checking Rules";

            if (Svc.Condition[ConditionFlag.Gathering])
            {
                IceLogging.Verbose("We're in the middle of gathering, so we're just going to go ahead and kick right over to that", tag);
                P.taskManager.Enqueue(() => GatheringInteract(), "Gathering at the node");
                return true;
            }

            if (leveInfo.GatheringRule is GatheringRule.Procurance)
            {
                IceLogging.Verbose("We just need to find the closest node to us that we can actually hit. Don't need to do any filtering", tag);
            }
            else
            {
                UpdateNodeList(leveInfo, routeInfo);
                IceLogging.Verbose("All node filters have been completed, so now we just need to actually travel to a node", tag);
            }
            P.taskManager.Enqueue(() => FindNode(leveInfo, routeInfo), "Finding our closest node");

            return true;
        }
        private static bool FindNode(LeveInfo.Leve_SheetData leveInfo, GatheringRoute routeInfo)
        {
            string tag = "Gathering: Finding Node";

            var navTask = P.navTask;

            if (Svc.Condition[ConditionFlag.Occupied33])
            {
                P.taskManager.Enqueue(() => WaitingForFanfare(), "Waiting for fanfare state");
                return true;
            }
            else if (!navTask.IsBusy)
            {
                var nodeClosest = routeInfo.NodeInfo
                    .Where(x => Player.DistanceTo(x.Position) < 3.7f)
                    .Where(x => IsTargetable(x))
                    .FirstOrDefault();

                if (nodeClosest != null)
                {
                    IceLogging.Verbose("We're at a valid node, so we just need to target -> interact", tag);
                    if (Svc.Condition[ConditionFlag.Gathering])
                    {
                        P.taskManager.Enqueue(() => GatheringInteract(), "Gathering at the node");
                        return true;
                    }
                    else
                    {
                        InteractWithNode(nodeClosest.BaseId);
                    }
                }
                else
                {
                    IceLogging.Verbose("We are not close to node, so going to do a check to see where the next valid one is", tag);
                    var validNode = Svc.Objects
                        .Where(x => !CheckedNodes.Contains(x.BaseId))
                        .Where(x => leveInfo.Gather_NodeInfo.NodeIds.Contains(x.BaseId))
                        .Where(x => x.IsTargetable)
                        .OrderBy(x => Player.DistanceTo(x.Position))
                        .FirstOrDefault();

                    if (validNode != null)
                    {
                        IceLogging.Verbose("We found a node that is within range and that we are able to gather at, so we're going to pathfind and gather there", tag);
                        var nodeInfo = routeInfo.NodeInfo.Where(x => x.BaseId == validNode.BaseId).FirstOrDefault();
                        if (nodeInfo != null)
                        {
                            IceLogging.Verbose("We found the node info in our route storage, traveling to it now", tag);
                            Task_Navmesh.Gathering_TravelToNode(nodeInfo);
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Error: No node found"))
                            {
                                IceLogging.Error("No node was found for this specific nodeId???? or atlast no information on where it is.", tag);
                                IceLogging.Error($"Leve: {Leve_Helper.LeveToGrab} | Node: {validNode.BaseId}", tag);
                            }
                        }
                    }
                    else
                    {
                        UpdateNodeList(leveInfo, routeInfo);

                        var closestNode = routeInfo.NodeInfo
                            .Where(x => !CheckedNodes.Contains(x.BaseId))
                            .FirstOrDefault();

                        IceLogging.Verbose("We are within a spot where there's not really a close node to us, so we're going to the closest one", tag);
                        if (closestNode != null)
                        {
                            IceLogging.Verbose("We found the node info in our route storage, traveling to it now", tag);
                            Task_Navmesh.Gathering_TravelToNode(closestNode);
                        }
                        else
                        {
                            if (EzThrottler.Throttle("Error: No node found"))
                            {
                                IceLogging.Verbose("No node was able to be grabbed that wasn't already checked off... something went wrong.", tag);
                                IceLogging.Verbose($"Current checked node count: {CheckedNodes.Count()} | Node Count: {routeInfo.NodeInfo.Count()}", tag);
                                IceLogging.Verbose($"Leve: {Leve_Helper.LeveToGrab}", tag);

                                IceLogging.Verbose("As a safety precaution, we're going to clear the list and let it re-attempt it", tag);
                                CheckedNodes.Clear();
                            }
                        }
                    }
                }
            }
            else
            {
                if (EzThrottler.Throttle("Travel log message", 3000))
                    IceLogging.Verbose("We are currently traveling, waiting patiently for us to finish", tag);
            }

            return false;
        }
        private static void UpdateNodeList(LeveInfo.Leve_SheetData leveInfo, GatheringRoute routeInfo)
        {
            if (leveInfo.GatheringRule is GatheringRule.Procurance)
                return;

            foreach (var node in Svc.Objects.Where(x => leveInfo.Gather_NodeInfo.NodeIds.Contains(x.BaseId)))
            {
                if (CheckedNodes.Contains(node.BaseId))
                    continue;

                if (node.IsTargetable)
                    continue;

                CheckedNodes.Add(node.BaseId);
            }
        }
        private static bool IsTargetable(GatheringNode node)
        {
            var selectedNode = Svc.Objects.Where(x => x.BaseId == node.BaseId)
                .Where(x => x.Position == node.Position)
                .FirstOrDefault();

            return selectedNode != null && selectedNode.IsTargetable;
        }
        private static void InteractWithNode(uint nodeId)
        {
            string tag = "Interacting with node";

            if (Utils.TryGetObjectByDataId(nodeId, out var gameObject))
            {
                if (EzThrottler.Throttle("Interact/Target NPC"))
                {
                    Utils.TargetgameObject(gameObject);
                    Utils.InteractWithObject(gameObject);
                }
            }
            else
            {
                if (EzThrottler.Throttle("Npc doesn't exist log", 2000))
                    IceLogging.Error($"Node: {nodeId} doesn't seem to exist?? this shoudl fix itself....\n" +
                                     $"Player Position: {Player.Position:N2}", tag);
            }
        }
        private static bool WaitingForFanfare()
        {
            string tag = "Gathering: Waiting for fanfare";

            if (P.navTask.IsBusy)
                P.navTask.Abort();

            if (P.navmesh.IsRunning())
                P.navmesh.Stop();

            if (GenericHelpers.TryGetAddonMaster<SelectYesno>(out var selectYesNo) && selectYesNo.IsAddonReady)
            {
                if (EzThrottler.Throttle("Selecting yes to teleport back"))
                    selectYesNo.Yes();

                return false;
            }
            else if (Svc.Condition[ConditionFlag.Occupied33] || Svc.Condition[ConditionFlag.BoundByDuty])
            {
                if (EzThrottler.Throttle("Fanfare waiting"))
                    IceLogging.Verbose("We're waiting for us to no longer be tied up", tag);
                return false;
            }
            else
            {
                Leve_Helper.State = LeveState.CheckLeves;
                return true;
            }
        }

        // - - - - Gathering Stuff - - - - //

        private static bool GatheringInteract()
        {
            string tag = "Gather: Gathering Interact";

            if (Svc.Condition[ConditionFlag.Gathering])
            {
                if (!Svc.Condition[ConditionFlag.ExecutingGatheringAction])
                {
                    if (GenericHelpers.TryGetAddonMaster<Gathering>(out var gathering) && gathering.IsAddonReady)
                    {
                        if (gathering.CurrentIntegrity == 0)
                        {
                            if (EzThrottler.Throttle("No Durability Check"))
                                IceLogging.Verbose("We're at 0 durability, which means we shouldn't gather. Waiting for us to get out of this state", tag);
                            return false;
                        }

                        var itemToGather = gathering.GatheredItems
                            .Where(x => x.ItemID != 0)
                            .OrderByDescending(x => x.GatherChance)
                            .ThenByDescending(x => x.ItemLevel)
                            .FirstOrDefault();

                        if (itemToGather != null)
                        {
                            if (CheckGatheringBuffs(gathering, itemToGather))
                                return false;

                            if (EzThrottler.Throttle("Gathering valid item"))
                                itemToGather.Gather();
                        }
                        else
                        {
                            itemToGather = gathering.GatheredItems.Where(x => x.ItemID != 0).First();

                            if (CheckGatheringBuffs(gathering, itemToGather))
                                return false;

                            if (EzThrottler.Throttle("Finding non-valid item"))
                            {
                                IceLogging.Verbose("We managed to not find a valid item... so we're just going to gather the first item we find", tag);
                                gathering.GatheredItems.Where(x => x.ItemID != 0).FirstOrDefault()?.Gather();
                            }
                        }
                    }
                }
                else
                {
                    if (EzThrottler.Throttle("Gathering: Waiting..."))
                        IceLogging.Verbose("Waiting for us to no longer be executing a gathering action anymore", tag);

                    if (BuffCounter is not Gather_Enums.Unknown)
                    {
                        CurrentUse[BuffCounter] += 1;
                        BuffCounter = Gather_Enums.Unknown;
                    }
                }
            }
            else
            {
                IceLogging.Verbose("We're no longer gathering. Going back to the start to see where we need to head to next", tag);
                foreach (var buff in CurrentUse)
                {
                    CurrentUse[buff.Key] = 0;
                }
                P.taskManager.EnqueueDelay(1000);
                return true;
            }

            return false;
        }

        private static Gather_Enums BuffCounter = Gather_Enums.Unknown;

        private static Dictionary<Gather_Enums, int> CurrentUse = new()
        {
            [Gather_Enums.BoonIncrease_1] = 0,
            [Gather_Enums.BoonIncrease_2] = 0,
            [Gather_Enums.Tidings] = 0,
            [Gather_Enums.YieldI] = 0,
            [Gather_Enums.YieldII] = 0,
            [Gather_Enums.BonusIntegrity] = 0,
            [Gather_Enums.BonusIntegrity_Chance] = 0,
            [Gather_Enums.BYII] = 0,
            [Gather_Enums.FieldMasteryI] = 0,
            [Gather_Enums.FieldMasteryII] = 0,
            [Gather_Enums.FieldMasteryIII] = 0,
            [Gather_Enums.FieldMasteryTemp] = 0,
            [Gather_Enums.TwelveBounty] = 0,
            [Gather_Enums.GivingLand] = 0,
            [Gather_Enums.Scrutiny] = 0,
            [Gather_Enums.Focus] = 0,
            [Gather_Enums.Priming] = 0,
            [Gather_Enums.Scour] = 0,
            [Gather_Enums.Brazen] = 0,
            [Gather_Enums.Meticulous] = 0,
        };

        private static bool CheckGatheringBuffs(Gathering gathering, Gathering.GatheredItem item)
        {
            string tag = "Gather: Check Buffs";

            var rule = LeveInfo.Leve_SheetInfo[Leve_Helper.LeveToGrab].GatheringRule;
            var profileId = C.RuleProfiles[rule];
            var playerLevel = Player.Level;
            var job = Player.Job;
            var currentGp = Utils.GetGp();

            bool missingDur = gathering.CurrentIntegrity != gathering.TotalIntegrity;

            var gatherProfile = C.FindGatherProfile(profileId) != null ? C.FindGatherProfile(profileId) : C.GatherProfiles[0];

            foreach (var buff in gatherProfile.BuffPriority)
            {
                var buffProfile = gatherProfile.GatheringBuffs[buff];
                var actionInfo = Gather_Util.gathActionDict[buff];
                var actionId = actionInfo.ClassAction[job].ActionId;
                var useCount = CurrentUse[buff];

                if (!buffProfile.Enabled)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not enabled", tag);
                    continue;
                }

                if (actionInfo.RequiredLv > playerLevel)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not not high enough lv", tag);
                    continue;
                }

                if (currentGp < actionInfo.RequiredGp)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to not enough GP", tag);
                    continue;
                }

                if (buffProfile.MaxUse != 0 && buffProfile.MaxUse > useCount)
                {
                    IceLogging.Verbose($"Skipping: {buff} due to max use not being 0: {buffProfile.MaxUse != 0} && maxUse being more than useCount", tag);
                    continue;
                }

                if (buff is Gather_Enums.BoonIncrease_1 or Gather_Enums.BoonIncrease_2)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    bool noBoonGain = item.BoonChance == 100;
                    bool minGp = buffProfile.GP_Min >= currentGp;

                    if (hasStatus || noBoonGain || missingDur)
                    {
                        IceLogging.Verbose($"We can't buff up for {buff}, continuing", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }

                    UseGatheringAction(actionId);
                    BuffCounter = buff;

                    return true;
                }
                else if (buff is Gather_Enums.Tidings)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    if (hasStatus)
                    {
                        IceLogging.Verbose($"Reporting you can't use: {buff}", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }
                    UseGatheringAction(actionId);
                    BuffCounter = buff;

                    return true;
                }
                else if (buff is Gather_Enums.YieldI or Gather_Enums.YieldII)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);
                    bool minimumInteg = buffProfile.Durability_MinUse != 0 && gathering.TotalIntegrity < buffProfile.Durability_MinUse;

                    if (hasStatus || minimumInteg)
                    {
                        IceLogging.Verbose($"We can't use {buff}, continuing onwards", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }

                    UseGatheringAction(actionId);
                    BuffCounter = buff;

                    return true;
                }
                else if (buff is Gather_Enums.BonusIntegrity)
                {
                    if (!missingDur)
                    {
                        IceLogging.Verbose($"We're not missing durability, so not using {buff}", tag);
                        continue;
                    }

                    if (buffProfile.Durability_MinUse != 0 && gathering.CurrentIntegrity < buffProfile.Durability_MinUse)
                    {
                        IceLogging.Verbose($"We were told to only use {buff} below {buffProfile.Durability_MinUse}", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }

                    UseGatheringAction(actionId);
                    BuffCounter = buff;

                    return true;
                }
                else if (buff is Gather_Enums.BonusIntegrity_Chance)
                {
                    bool missingStatus = !Utils.HasStatusId(actionInfo.StatusId);
                    if (missingStatus && !missingDur)
                    {
                        IceLogging.Verbose($"We don't have bonus integ status, or we're not missing durability, so not using {buff}", tag);
                        continue;
                    }

                    if (buffProfile.Durability_MinUse != 0 && gathering.CurrentIntegrity < buffProfile.Durability_MinUse)
                    {
                        IceLogging.Verbose($"We were told to only use {buff} below {buffProfile.Durability_MinUse}", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }

                    UseGatheringAction(actionId);
                    // return true;
                }
                else if (buff is Gather_Enums.BYII)
                {
                    var status1 = Utils.HasStatusId(actionInfo.StatusId);
                    var status2 = Utils.HasStatusId(actionInfo.StatusId2);

                    bool hasStatus = status1 || status2;
                    if (hasStatus)
                    {
                        IceLogging.Verbose($"Reporting you can't use {buff}", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"{buff} is valid to use", tag);
                    }

                    UseGatheringAction(actionId);
                    // return true;
                }
                else if (buff is Gather_Enums.FieldMasteryI or Gather_Enums.FieldMasteryII or Gather_Enums.FieldMasteryIII)
                {
                    bool hasStatus = Utils.HasStatusId(actionInfo.StatusId);

                    if (hasStatus)
                    {
                        IceLogging.Verbose($"Skipping {buff} due to having status", tag);
                        continue;
                    }
                    
                    if (buff != PickFieldMasterySkill(gatherProfile, playerLevel, currentGp, item.GatherChance))
                    {
                        IceLogging.Verbose($"Told this buff wasn't the best use of mastery skills {buff}", tag);
                        continue;
                    }
                    else
                    {
                        IceLogging.Verbose($"Using buff: {buff}", tag);
                    }

                    UseGatheringAction(actionId);
                    return true;
                }
            }

            return false;
        }

        private static Gather_Enums? PickFieldMasterySkill(Config.GatherProfile gatherProfile, int playerLevel, int availableGp, int currentChance)
        {
            bool CanUseBuff(Gather_Enums buff)
            {
                if (!gatherProfile.GatheringBuffs.TryGetValue(buff, out var settings))
                    return false;

                if (!settings.Enabled)
                    return false;

                var actionInfo = Gather_Util.gathActionDict[buff];

                if (settings.GP_Min > availableGp)
                    return false;

                if (availableGp < actionInfo.RequiredGp)
                    return false;

                if (playerLevel < actionInfo.RequiredLv)
                    return false;

                if (settings.MaxUse != 0 && CurrentUse[buff] >= settings.MaxUse)
                    return false;

                return true;
            }

            bool mastery1 = CanUseBuff(Gather_Enums.FieldMasteryI);
            bool mastery2 = CanUseBuff(Gather_Enums.FieldMasteryII);
            bool mastery3 = CanUseBuff(Gather_Enums.FieldMasteryIII);

            // Already at 100%? No skill needed, so continuing on
            if (currentChance >= 100)
                return null;

            int neededBonus = 100 - currentChance;

            // Find the cheapest skill that gets us to 100%
            if (neededBonus <= 5 && availableGp >= 50 && mastery1)
                return Gather_Enums.FieldMasteryI;

            if (neededBonus <= 15 && availableGp >= 100 && mastery2)
                return Gather_Enums.FieldMasteryII;

            if (neededBonus <= 50 && availableGp >= 250 && mastery3)
                return Gather_Enums.FieldMasteryIII;

            // If we can't reach 100%, use the best skill we can afford
            if (availableGp >= 250 && mastery3)
                return Gather_Enums.FieldMasteryIII;

            if (availableGp >= 100 && mastery2)
                return Gather_Enums.FieldMasteryII;

            if (availableGp >= 50 && mastery1)
                return Gather_Enums.FieldMasteryI;

            return null; // Can't afford any skill
        }

        private static unsafe void UseGatheringAction(uint ActionId)
        {
            if (EzThrottler.Throttle("Using Greater Reach", 500))
                ActionManager.Instance()->UseAction(ActionType.Action, ActionId);
        }
    }
}
