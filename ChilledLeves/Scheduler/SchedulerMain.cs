using ChilledLeves.Scheduler.Tasks;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

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
                        C.workList.Add(new LeveEntry { LeveID = kpd.LeveID, InputValue = kpd.InputValue, ItemAmount = 0 });
                    }
                    else if (kpd.InputValue != 0)
                        C.workList.Add(new LeveEntry { LeveID = kpd.LeveID, InputValue = kpd.InputValue, ItemAmount = 0 });
                }
            }

            ListCycled.Clear();
            return true;
        }

        internal static string CurrentProcess = "";
        internal static bool KeepLeves = false;
        internal static bool GatheringMode = false;
        internal static bool WorkListMode = false;

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
                            #nullable disable
                            uint leve = 0;
                            bool hasLeves = false;
                            if (C.workList.Count > 0)
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
                                            ListCycled.Add(new LeveEntry { LeveID = entry.LeveID, InputValue = 0, ItemAmount = 0 });
                                            PluginDebug($"List Cycled entry added {entry.LeveID}");
                                        }
                                        C.workList.Remove(entry);
                                    }
                                    else if (entry.InputValue != 0)
                                    {
                                        PluginVerbos($"Worklist has found that: {entry.LeveID} has an amount that isn't set to 0. Starting to work on this turnin process");
                                        hasLeves = true;
                                        var templeve = entry.LeveID;
                                        var currentAmount = GetItemCount((int)CrafterLeves[templeve].ItemID);
                                        var necessaryAmount = CrafterLeves[templeve].TurninAmount;
                                        if (currentAmount >= necessaryAmount)
                                        {
                                            PluginVerbos("You have the necessary amount to run this leve. Grabbing/Turning In");
                                            leve = templeve;
                                            break;
                                        }
                                        else
                                        {
                                            PluginVerbos("You do not have the amount to complete this turnin, skipping leve");
                                            if (!ListCycled.Any(e => e.LeveID == entry.LeveID))
                                            {
                                                ListCycled.Add(new LeveEntry { LeveID = entry.LeveID, InputValue = entry.InputValue, ItemAmount = 0 });
                                                PluginDebug($"List Cycled entry added {entry.LeveID}");
                                            }
                                            C.workList.Remove(entry);
                                        }
                                    }
                                }
                                if (hasLeves)
                                {
                                    if (IsAccepted(leve))
                                    {
                                        var npc = CrafterLeves[leve].LeveTurninVendorID;
                                        var LeveName = CrafterLeves[leve].LeveName;
                                        var zoneID = LeveNPCDict[npc].ZoneID;
                                        var aetheryte = LeveNPCDict[npc].Aetheryte;
                                        var NPCLocation = LeveNPCDict[npc].NPCInteractZone;
                                        var NpcInteractZone = LeveNPCDict[npc].NPCInteractZone;

                                        if (IsInZone(zoneID))
                                        {
                                            if (Player.DistanceTo(NPCLocation) < 6.8f)
                                            {
                                                P.taskManager.Enqueue(() => PluginVerbos("Close to the NPC, Starting Turnin Process"));
                                                TaskTurninMulti.Enqueue(zoneID);
                                            }
                                            else if (Player.DistanceTo(NPCLocation) > 6.8f)
                                            {
                                                bool fly = false;

                                                if (LeveNPCDict[npc].Mount && Player.DistanceTo(NPCLocation) > 20)
                                                {
                                                    TaskMountUp.Enqueue();
                                                    if (Player.DistanceTo(NPCLocation) > 50)
                                                    {
                                                        fly = true;
                                                    }
                                                    else if (LeveNPCDict[npc].Fly.HasValue)
                                                    {
                                                        fly = true;
                                                    }
                                                }

                                                TaskMoveTo.Enqueue(NpcInteractZone, "LeveNPC", fly, 0.5f);
                                            }
                                        }
                                        else if (!IsInZone(zoneID))
                                        {
                                            TaskTeleport.Enqueue(aetheryte, zoneID);
                                        }
                                    }
                                    else if (!IsAccepted(leve) && Allowances > 0)
                                    {
                                        var npc = CrafterLeves[leve].LeveVendorID;
                                        var zoneID = LeveNPCDict[npc].ZoneID;
                                        var aetheryte = LeveNPCDict[npc].Aetheryte;
                                        var NPCLocation = LeveNPCDict[npc].NPCInteractZone;
                                        var requiredLevel = CrafterLeves[leve].Level;
                                        var jobID = CrafterLeves[leve].EcomJob;
                                        var buttonSelected = 0;
                                        if (GatheringJobList.Contains((int)jobID))
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
                                            if (GetDistanceToPlayer(NPCLocation) < 1)
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
                                            else if (GetDistanceToPlayer(NPCLocation) > 1)
                                            {
                                                bool fly = false;

                                                if (LeveNPCDict[npc].Mount)
                                                {
                                                    TaskMountUp.Enqueue();
                                                    if (LeveNPCDict[npc].Fly.HasValue)
                                                    {
                                                        fly = (bool)LeveNPCDict[npc].Fly;
                                                    }
                                                }

                                                TaskMoveTo.Enqueue(NPCLocation, "LeveNPC", fly, 0.5f);
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
                            Job ecomJob = EcomJobFinder(IconSlot);
                            TaskClassChange.Enqueue(ecomJob);

                            uint leveId = 0;
                            bool isHighEnough = false;
                            bool isReadyforTurnin = false;
                            bool canGrab = false;
                            var classLevel = UIState.Instance()->PlayerState.ClassJobLevels;

                            if (C.RunUntilSelected == "Lv." && Player.GetUnsyncedLevel(ecomJob) == C.LevelSliderInput)
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

                                    if (CrafterLeves[leveId].Level <= Player.GetUnsyncedLevel(ecomJob))
                                    {
                                        if (!isHighEnough)
                                            isHighEnough = true;

                                        var itemId = CrafterLeves[leveId].ItemID;
                                        var currentAmount = GetItemCount((int)itemId);
                                        var turninAmount = CrafterLeves[leveId].TurninAmount;
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
                                            PluginVerbos($"LeveId: {leveId} | leveName: {CrafterLeves[leveId].LeveName} has enough, but hasn't been accepted yet");
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
                                    var Turninnpc = CrafterLeves[leveId].LeveTurninVendorID;
                                    var LeveName = CrafterLeves[leveId].LeveName;
                                    var zoneID = LeveNPCDict[Turninnpc].ZoneID;
                                    var aetheryte = LeveNPCDict[Turninnpc].Aetheryte;
                                    var NPCLocation = LeveNPCDict[Turninnpc].NPCInteractZone;

                                    if (IsInZone(zoneID))
                                    {
                                        if (GetDistanceToPlayer(NPCLocation) < 1)
                                        {
                                            TaskTarget.Enqueue(Turninnpc);
                                            TaskInteract.Enqueue(Turninnpc);
                                            TaskTurnin.Enqueue(LeveName, leveId);
                                            TaskUpdateWorkList.Enqueue(leveId);
                                        }
                                        else if (GetDistanceToPlayer(NPCLocation) > 1)
                                        {
                                            bool fly = false;

                                            if (LeveNPCDict[Turninnpc].Mount)
                                            {
                                                TaskMountUp.Enqueue();
                                                if (LeveNPCDict[Turninnpc].Fly.HasValue)
                                                {
                                                    fly = (bool)LeveNPCDict[Turninnpc].Fly;
                                                }
                                            }

                                            TaskMoveTo.Enqueue(NPCLocation, "LeveNPC", fly, 1);
                                        }
                                    }
                                    else if (!IsInZone(zoneID))
                                    {
                                        TaskTeleport.Enqueue(aetheryte, zoneID);
                                    }
                                }
                                else if (canGrab && Allowances > 0)
                                {
                                    var npc = CrafterLeves[leveId].LeveVendorID;
                                    var zoneID = LeveNPCDict[npc].ZoneID;
                                    var aetheryte = LeveNPCDict[npc].Aetheryte;
                                    var NpcInteractZone = LeveNPCDict[npc].NPCInteractZone;
                                    var NpcLocation = LeveNPCDict[npc].NPCLocation;
                                    var requiredLevel = CrafterLeves[leveId].Level;
                                    var jobID = CrafterLeves[leveId].EcomJob;
                                    var buttonSelected = LeveNPCDict[npc].GatheringButton;

                                    if (IsInZone(zoneID))
                                    {
                                        if (Player.DistanceTo(NpcLocation) < 6.8f)
                                        {
                                            TaskTarget.Enqueue(npc);
                                            TaskGrabPrioLeve.Enqueue(npc, buttonSelected);
                                        }
                                        else if (Player.DistanceTo(NpcLocation) > 6.8f)
                                        {
                                            bool fly = false;

                                            if (LeveNPCDict[npc].Mount && Player.DistanceTo(NpcLocation) > 20)
                                            {
                                                TaskMountUp.Enqueue();
                                                if (Player.DistanceTo(NpcLocation) > 50)
                                                {
                                                    fly = true;
                                                }
                                                else if (LeveNPCDict[npc].Fly.HasValue)
                                                {
                                                    fly = true;
                                                }
                                            }

                                            TaskMoveTo.Enqueue(NpcInteractZone, "LeveNPC", fly, 0.5f);
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
