using ChilledLeves.Scheduler.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

#nullable disable
namespace ChilledLeves.Scheduler
{
    internal static unsafe class SchedulerMain
    {
        internal static bool AreWeTicking;
        internal static bool EnableTicking
        {
            get => AreWeTicking;
            private set => AreWeTicking = value;
        }
        internal static bool EnablePlugin()
        {
            EnableTicking = true;
            return true;
        }
        internal static bool DisablePlugin()
        {
            EnableTicking = false;
            GatheringMode = false;
            WorkListMode = false;
            P.navmesh.Stop();
            P.taskManager.Abort();
            
            foreach (var kpd in ListCycled)
            {
                if (!C.workList.Any(e => e.LeveID == kpd.LeveID))
                {
                    if (KeepLeves)
                    {
                        if (kpd.InputValue == 0)
                            kpd.InputValue = 1;
                        C.workList.Add(new LeveEntry { LeveID = kpd.LeveID, InputValue = kpd.InputValue});
                    }
                    else if (kpd.InputValue != 0)
                        C.workList.Add(new LeveEntry { LeveID = kpd.LeveID, InputValue = kpd.InputValue});
                }
            }

            ListCycled.Clear();
            return true;
        }

        internal static string CurrentProcess = "";
        internal static bool KeepLeves = false;
        internal static bool GatheringMode = false;
        internal static bool WorkListMode = false;
        private static int MinMountDistance = 25;
        private static float InteractDistance = 6.8f;

        internal static void HandleMountAndMove(Vector3 NpcLocation, uint npcId)
        {
            bool fly = false;
            bool mount = LeveNPCDict[npcId].Mount;
            Vector3 NpcInteractZone = LeveNPCDict[npcId].NPCInteractZone;

            // if mounting is required/is an option
            if (mount && Player.DistanceTo(NpcLocation) > 25)
            {
                TaskMountUp.Enqueue();
                if (LeveNPCDict[npcId].Fly.HasValue && Player.DistanceTo(NpcLocation) > 50)
                {
                    fly = true;
                }
            }

            // if the player is too far away from the NPC
            if (Player.DistanceTo(NpcLocation) >= InteractDistance)
            {
                TaskMoveTo.Enqueue(NpcInteractZone, "LeveNPC", fly, 0.5f);
            }
        }

        internal static void Tick()
        {
            if (AreWeTicking)
            {
                if (GenericThrottle)
                {
                    if (!P.taskManager.IsBusy)
                    {
                        if (WorkListMode)
                        {
                            uint leve = 0;
                            bool readyForTurnin = false;
                            bool hasLeves = false;
                            if (C.workList.Count > 0)
                            {
                                // Checking initially to see if any leves can be turned in
                                foreach (var entry in C.workList)
                                {
                                    if (entry.InputValue == 0)
                                    {
                                        PluginDebug($"LeveID: {entry.LeveID} is 0, adding to Leve Entry");
                                        if (!ListCycled.Any(e => e.LeveID == entry.LeveID))
                                        {
                                            if (entry.InputValue == 0)
                                                entry.InputValue = 1;
                                            ListCycled.Add(new LeveEntry { LeveID = entry.LeveID, InputValue = 0 });
                                            PluginDebug($"List Cycled entry added {entry.LeveID}");
                                        }
                                        C.workList.Remove(entry);
                                    }
                                    else if (entry.InputValue != 0)
                                    {
                                        if (!readyForTurnin)
                                        {
                                            PluginVerbos($"Checking to see if the leve can be grabbed/turned in. LeveID: {entry.LeveID}");
                                            var currentAmount = GetItemCount((int)CraftDictionary[entry.LeveID].ItemID);
                                            var necessaryAmount = CraftDictionary[entry.LeveID].TurninAmount;

                                            if (IsAccepted(entry.LeveID) && currentAmount >= necessaryAmount)
                                            {
                                                PluginVerbos($"LeveID: {entry.LeveID} has been accepted. Checking to see if it can be turned in");
                                                leve = entry.LeveID;
                                                readyForTurnin = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (!readyForTurnin)
                                {
                                    foreach (var entry in C.workList)
                                    {
                                        if (entry.InputValue == 0)
                                        {
                                            PluginDebug($"LeveID: {entry.LeveID} is 0, adding to Leve Entry");
                                            if (!ListCycled.Any(e => e.LeveID == entry.LeveID))
                                            {
                                                if (entry.InputValue == 0)
                                                    entry.InputValue = 1;
                                                ListCycled.Add(new LeveEntry { LeveID = entry.LeveID, InputValue = 0 });
                                                PluginDebug($"List Cycled entry added {entry.LeveID}");
                                            }
                                            C.workList.Remove(entry);
                                        }
                                        else if (entry.InputValue != 0)
                                        {
                                            PluginVerbos($"Worklist has found that: {entry.LeveID} has an amount that isn't set to 0. Starting to work on this turnin process");
                                            var templeve = entry.LeveID;
                                            var currentAmount = GetItemCount((int)CraftDictionary[templeve].ItemID);
                                            var necessaryAmount = CraftDictionary[templeve].TurninAmount;
                                            bool isLevel = LeveDictionary[templeve].Level <= Player.GetLevel((Job)LeveDictionary[templeve].EcomJob);
                                            bool canTurnin = currentAmount >= necessaryAmount;

                                            if (canTurnin && isLevel)
                                            {
                                                PluginVerbos("You have the necessary amount/is the proper level to run this leve. Grabbing/Turning In");
                                                hasLeves = true;
                                                leve = templeve;
                                                break;
                                            }
                                            else if (!canTurnin || !isLevel)
                                            {
                                                PluginVerbos("You do not have the amount to complete this turnin, skipping leve");
                                                if (!ListCycled.Any(e => e.LeveID == entry.LeveID))
                                                {
                                                    ListCycled.Add(new LeveEntry { LeveID = entry.LeveID, InputValue = entry.InputValue });
                                                    PluginDebug($"List Cycled entry added {entry.LeveID}");
                                                }
                                                C.workList.Remove(entry);
                                            }
                                        }
                                    }
                                }
                                if (hasLeves || readyForTurnin)
                                {
                                    PluginLog.Information($"Cost: {LeveDictionary[leve].AllowanceCost} | Current Amount: {Allowances}");

                                    if (IsAccepted(leve))
                                    {
                                        var npc = CraftDictionary[leve].LeveTurninVendorID;
                                        var zoneID = LeveNPCDict[npc].ZoneID;
                                        var aetheryte = LeveNPCDict[npc].Aetheryte;
                                        var NPCLocation = LeveNPCDict[npc].NPCLocation;

                                        if (IsInZone(zoneID))
                                        {
                                            P.taskManager.Enqueue(() => PluginInfo("Player is the correct zone, checking to see if they're in the right distance"));
                                            P.taskManager.Enqueue(() => PluginInfo($"Distance to NPC: {Player.DistanceTo(NPCLocation)} Interact Distance: {InteractDistance} \n" +
                                                                                   $"Npc Location: {NPCLocation:N2} | Player Location: {Player.Position:N2}"));
                                            if (Player.DistanceTo(NPCLocation) <= InteractDistance)
                                            {
                                                P.taskManager.Enqueue(() => PluginInfo("Close to the NPC, Starting Turnin Process"));
                                                TaskTurninMulti.Enqueue(zoneID);
                                            }
                                            else 
                                            {
                                                HandleMountAndMove(NPCLocation, npc);
                                            }
                                        }
                                        else if (!IsInZone(zoneID))
                                        {
                                            TaskTeleport.Enqueue(aetheryte, zoneID);
                                        }
                                    }
                                    else if (!IsAccepted(leve) && (LeveDictionary[leve].AllowanceCost <= Allowances))
                                    {
                                        PluginLog.Information($"Leve Allowance is {Allowances}, Cost of the leve is: {LeveDictionary[leve].AllowanceCost}");

                                        var npc = LeveDictionary[leve].LeveVendorID;
                                        var zoneID = LeveNPCDict[npc].ZoneID;
                                        var aetheryte = LeveNPCDict[npc].Aetheryte;
                                        var NPCLocation = LeveNPCDict[npc].NPCLocation;
                                        var requiredLevel = LeveDictionary[leve].Level;
                                        var jobID = LeveDictionary[leve].EcomJob;
                                        var buttonSelected = 0;
                                        if (LeveDictionary[leve].AllowanceCost == 10)
                                        {
                                            if (CrafterJobList.Contains((int)jobID))
                                            {
                                                buttonSelected = LeveNPCDict[npc].LSCrafterButton;
                                            }
                                            else if (GatheringJobList.Contains((int)jobID))
                                            {
                                                buttonSelected = LeveNPCDict[npc].LSGatherButton;
                                            }
                                        }
                                        else if (GatheringJobList.Contains((int)jobID))
                                        {
                                            buttonSelected = LeveNPCDict[npc].GatheringButton;
                                        }
                                        else if (CrafterJobList.Contains((int)jobID))
                                        {
                                            buttonSelected = LeveNPCDict[npc].CrafterButton;
                                        }
                                        TaskClassChange.Enqueue(jobID);

                                        if (IsInZone(zoneID))
                                        {
                                            if (Player.DistanceTo(NPCLocation) < InteractDistance)
                                            {
                                                TaskTarget.Enqueue(npc);
                                                if (!C.GrabMulti)
                                                {
                                                    TaskGrabLeve.Enqueue(leve, npc, buttonSelected);
                                                }
                                                else if (C.GrabMulti)
                                                {
                                                    TaskGrabMultiLeves.Enqueue(npc, buttonSelected);
                                                }
                                                if (C.IncreaseDelay)
                                                    P.taskManager.EnqueueDelay(1000);
                                            }
                                            else 
                                            {
                                                HandleMountAndMove(NPCLocation, npc);
                                            }
                                        }
                                        else if (!IsInZone(zoneID))
                                        {
                                            P.taskManager.EnqueueDelay(500);
                                            TaskLevelChecker.Enqueue(requiredLevel);
                                            TaskTeleport.Enqueue(aetheryte, zoneID);
                                        }
                                    }
                                    else if (Allowances == 0)
                                    {
                                        PluginVerbos("Somehow, you got here??? Not sure how but. Disabling the plugin");
                                        DisablePlugin();
                                    }
                                }
                            }
                            else
                            {
                                if (Allowances == 0)
                                {
                                    PluginVerbos("HEY. YOU HAVE NO LEVES FOOL.");
                                }
                                else if (C.workList.Count == 0)
                                {
                                    PluginVerbos("You have completed your list, stopping the plugin");
                                }
                                DisablePlugin();
                            }
                        }
                        else if (GatheringMode)
                        {
                            Job ecomJob = EcomJobFinder(C.ClassJobType);
                            TaskClassChange.Enqueue(ecomJob);

                            uint leveId = 0;
                            bool isHighEnough = false;
                            bool isReadyforTurnin = false;
                            bool canGrab = false;
                            var classLevel = UIState.Instance()->PlayerState.ClassJobLevels;

                            if (C.RunUntilSelected == "Lv." && Player.GetLevel(ecomJob) >= C.LevelSliderInput)
                            {
                                string ChatText = "You have hit the level set, stopping the farming process";
                                PluginVerbos(ChatText);
                                Svc.Chat.Print(ChatText);
                                DisablePlugin();
                            }
                            else if (C.RunUntilSelected == "All Leves Complete" && AllCompleted())
                            {
                                string ChatText = "All leves have been completed on for this class, topping the process";
                                PluginVerbos(ChatText);
                                Svc.Chat.Print(ChatText);
                                DisablePlugin();
                            }
                            else
                            {
                                foreach (var leve in SelectedLeves)
                                {
                                    leveId = leve;

                                    if (LeveDictionary[leveId].Level <= Player.GetLevel(ecomJob))
                                    {
                                        if (!isHighEnough)
                                            isHighEnough = true;

                                        var itemId = CraftDictionary[leveId].ItemID;
                                        var currentAmount = GetItemCount((int)itemId);
                                        var turninAmount = CraftDictionary[leveId].TurninAmount;
                                        var questCheck = HasAcceptedLeve();
                                        if (questCheck != 0 && IsAccepted(questCheck))
                                        {
                                            PluginVerbos($"LeveId: {leveId} has been found and can be turned in. Proceeding onto the turnin process");
                                            leveId = HasAcceptedLeve();
                                            isReadyforTurnin = true;
                                            break;
                                        }
                                        else if (currentAmount >= turninAmount)
                                        {
                                            PluginVerbos($"LeveId: {leveId} | leveName: {LeveDictionary[leveId].LeveName} has enough, but hasn't been accepted yet");
                                            canGrab = true;
                                            break;
                                        }
                                    }
                                }

                                if (!isReadyforTurnin && canGrab)
                                {
                                    PluginVerbos("No leves are ready for turnin, but you can grab a leve thankfully");
                                }

                                if (isReadyforTurnin)
                                {
                                    var Turninnpc = CraftDictionary[leveId].LeveTurninVendorID;
                                    var LeveName = LeveDictionary[leveId].LeveName;
                                    var zoneID = LeveNPCDict[Turninnpc].ZoneID;
                                    var aetheryte = LeveNPCDict[Turninnpc].Aetheryte;
                                    var NPCLocation = LeveNPCDict[Turninnpc].NPCLocation;

                                    if (IsInZone(zoneID))
                                    {
                                        if (Player.DistanceTo(NPCLocation) < InteractDistance)
                                        {
                                            TaskTarget.Enqueue(Turninnpc);
                                            TaskInteract.Enqueue(Turninnpc);
                                            TaskTurnin.Enqueue(LeveName, leveId);
                                            TaskUpdateWorkList.Enqueue(leveId);
                                        }
                                        else
                                        {
                                            HandleMountAndMove(NPCLocation, Turninnpc);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                }
                                else if (canGrab && LeveDictionary[leveId].AllowanceCost <= Allowances)
                                {
                                    var npc = LeveDictionary[leveId].LeveVendorID;
                                    var zoneID = LeveNPCDict[npc].ZoneID;
                                    var aetheryte = LeveNPCDict[npc].Aetheryte;
                                    var NpcInteractZone = LeveNPCDict[npc].NPCInteractZone;
                                    var NpcLocation = LeveNPCDict[npc].NPCLocation;
                                    var requiredLevel = LeveDictionary[leveId].Level;
                                    var jobID = LeveDictionary[leveId].EcomJob;
                                    var buttonSelected = 0;

                                    if (LeveDictionary[leveId].AllowanceCost == 10)
                                    {
                                        if (CrafterJobList.Contains((int)jobID))
                                        {
                                            buttonSelected = LeveNPCDict[npc].LSCrafterButton;
                                        }
                                        else if (GatheringJobList.Contains((int)jobID))
                                        {
                                            buttonSelected = LeveNPCDict[npc].LSGatherButton;
                                        }
                                    }
                                    else
                                    {
                                        buttonSelected = LeveNPCDict[npc].GatheringButton;
                                    }

                                    if (IsInZone(zoneID))
                                    {
                                        if (Player.DistanceTo(NpcLocation) <= 6.8f)
                                        {
                                            TaskTarget.Enqueue(npc);
                                            TaskGrabPrioLeve.Enqueue(npc, buttonSelected);
                                        }
                                        else
                                        {
                                            HandleMountAndMove(NpcLocation, npc);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        PluginVerbos("Not in the correct zone, heading to the proper one.");
                                        PluginVerbos($"NPC Id = {npc}");
                                        PluginVerbos($"NPC Name: {LeveNPCDict[npc].Name}");
                                        PluginVerbos($"Zone: {ZoneName(LeveNPCDict[npc].ZoneID)}");
                                        TaskClassChange.Enqueue(jobID);
                                        P.taskManager.EnqueueDelay(500);
                                        TaskLevelChecker.Enqueue(requiredLevel);
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                }
                                else
                                {
                                    PluginVerbos("Stopping the plugin, no potentional leves");
                                    SchedulerMain.DisablePlugin();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
