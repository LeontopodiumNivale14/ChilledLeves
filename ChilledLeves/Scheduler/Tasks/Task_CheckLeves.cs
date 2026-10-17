using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using ECommons.GameHelpers;
using System.Collections.Generic;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class Task_CheckLeves
    {
        public static void Enqueue()
        {
            P.taskManager.Enqueue(() => Check_CurrentLeves(), "Checking Active Leves");
        }

        private static bool Check_CurrentLeves()
        {
            string tag = "Check Active Leves";

            Leve_Helper.LeveToGrab = 0;
            uint turninLeve = 0;

            var currentLeves = Utils.Leve_ActiveIds();
            List<uint> activeGatheringLeves = [];

            var npcInfo = C.ARR_LevemetPriority[C.ARR_NpcId];
            var npcList = npcInfo.Where(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == C.ARR_SelectedAssignment);


            foreach (var leve in currentLeves)
            {
                if (C.LeveOrder.Contains(leve) || (Leve_Helper.SelectedMode == ModeSelection.ARR_Grind && npcList.Contains(leve)))
                {
                    if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo))
                    {
                        if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                        {
                            var materialInfo = sheetInfo.MaterialInfo;
                            var neededAmount = Utils.Leve_RequiredAmount(materialInfo);

                            if (Utils.GetItemCount(materialInfo.Item_Id) >= neededAmount)
                            {
                                if (turninLeve == 0)
                                {
                                    IceLogging.Debug($"Found a leve that we have enough for a turnin. Registering it now so it'll be stored. {leve}", tag);
                                    IceLogging.Debug($"We may not turn this in immediately if we have gathering leves to still do though", tag);
                                    turninLeve = leve;
                                }
                                continue;
                            }
                        }
                        else if (LeveInfo.LeveJobs_Gathering.Contains(sheetInfo.Job))
                        {
                            var currentSeq = Utils.Leve_Sequence(leve);

                            if (currentSeq is 1 or 3)
                            {
                                IceLogging.Debug($"Leve [{leve}] still needs to be completed/redone. Adding it to the post checks", tag);
                                activeGatheringLeves.Add(leve);
                            }
                            else if (currentSeq is 255)
                            {
                                if (turninLeve == 0)
                                {
                                    IceLogging.Debug($"Found a leve that we have completed for a turnin. Registering it now so it'll be stored. {leve}", tag);
                                    IceLogging.Debug($"We may not turn this in immediately if we have gathering leves to still do though", tag);
                                    turninLeve = leve;
                                }
                                continue;
                            }
                        }
                    }
                }
            }

            if (activeGatheringLeves.Count > 0)
            {
                var leve = activeGatheringLeves.First();
                IceLogging.Debug($"We found a gathering leve that is in need of completion, going to go do so {leve}", tag);

                Leve_Helper.LeveToGrab = leve;
                Leve_Helper.State = LeveState.GatheringLeve_Start;

                return true;
            }

            if (turninLeve != 0)
            {
                IceLogging.Debug("All checks have been completed. We don't have any gathering leves that we need to attemp so, we just going to turnin", tag);
                IceLogging.Debug($"Setting turnin leve to: {turninLeve}, and proceeding to turnin", tag);
                Leve_Helper.LeveToGrab = turninLeve;
                Leve_Helper.State = LeveState.Turnin_Leve;
                return true;
            }
            else
            {
                IceLogging.Debug("No active leves were found that were in our list, so we're going to instead find one to complete", tag);
                if (Leve_Helper.SelectedMode is ModeSelection.Standard)
                {
                    IceLogging.Debug("Mode is currently in standard, going to check our listing for leves", tag);
                    P.taskManager.Enqueue(() => Check_StandardLeves(), "Checking Standard Leves");
                    return true;
                }
                else if (Leve_Helper.SelectedMode is ModeSelection.ARR_Grind)
                {
                    IceLogging.Debug("Mode is in the ARR Grind mode [Priority Leves], going to check to see if we have those items atleast", tag);
                    P.taskManager.Enqueue(() => Check_ARRLeves(), "Checking Priority Leves");
                    return true;
                }
                else
                {
                    IceLogging.Error($"We've ran into an invalid state for our mode selection? Current mode is: {Leve_Helper.SelectedMode}. Stopping the process", tag);
                    Leve_Helper.State = LeveState.Idle;
                    P.taskManager.Tasks.Clear();
                    return true;
                }
            }
        }
        private static bool Check_StandardLeves()
        {
            string tag = "Check_StandardLeves";

            Leve_Helper.LeveToGrab = 0;

            List<uint> levesToRemove = new();
            foreach (var leve in C.LeveOrder)
            {
                if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo))
                {
                    if (C.LeveList[leve] > 0)
                    {
                        if (!Utils.EnoughAllowance(leve))
                        {
                            IceLogging.Verbose($"Skipping the leve because we don't have enough currency for it: {leve}", tag);
                        }

                        if (Utils.PotentionalLeve(leve, tag))
                        {
                            IceLogging.Verbose($"Found a leve that isnt' at a count of 0! And hopefully it's one we can actually grab...", tag);
                            IceLogging.Verbose($"Queueing up the following leve: [{leve}] | Name: {sheetInfo.LeveName}", tag);
                            Leve_Helper.LeveToGrab = leve;
                            break;
                        }
                    }
                    else
                    {
                        IceLogging.Verbose($"Leve count has hit 0: {leve}, removing", tag);
                        levesToRemove.Add(leve);
                    }
                }
                else
                {
                    IceLogging.Error($"Somehow found a leve that was added to the leve order w/o existing in the sheets??? {leve}, adding to remove post checking the rest", tag);
                    levesToRemove.Add(leve);
                }
            }
            foreach (var leve in levesToRemove)
            {
                C.LeveOrder.Remove(leve);
            }
            if (levesToRemove.Count > 0)
            {
                C.Save();
            }

            if (Leve_Helper.LeveToGrab == 0)
            {
                IceLogging.Debug("We've found no leves that we're able to complete, so we're just going to stop the process", tag);
                Leve_Helper.State = LeveState.Idle;
                return true;
            }
            else
            {
                IceLogging.Verbose("Swapping to traveling to grab said leve", tag);
                if (Leve_Helper.SelectedMode is ModeSelection.ARR_Grind)
                    Leve_Helper.State = LeveState.Grab_ARRLeve;
                else if (Leve_Helper.SelectedMode is ModeSelection.Standard)
                    Leve_Helper.State = LeveState.Grab_StandardLeve;

                return true;
            }
        }

        private static bool Check_ARRLeves()
        {
            string tag = "Check Leves: ARR Grind";

            var arrList = Leve_Helper.ValidARRLeves;

            arrList.Clear();
            var selectedNpc = C.ARR_NpcId;
            var job = C.ARR_SelectedAssignment;
            var condition = C.ARR_StopCondition;

            if (C.ARR_LevemetPriority.TryGetValue(selectedNpc, out var npcList))
            {
                int cost = 0;
                foreach (var leve in npcList.Where(x => LeveInfo.Leve_SheetInfo[x].JobAssignmentType == job))
                {
                    var sheetInfo = LeveInfo.Leve_SheetInfo[leve];
                    if (cost == 0)
                        cost = sheetInfo.AllowanceCost;

                    if (cost != sheetInfo.AllowanceCost)
                        continue;

                    if (condition == StopConditions.Complete && Utils.Leve_IsComplete(leve))
                        continue;

                    if (Player.GetLevel(sheetInfo.Job) < sheetInfo.Level)
                        continue;

                    if (Utils.PotentionalLeve(leve, "ARR Grind"))
                        arrList.Add(leve);
                }

                if (arrList.Count != 0)
                {
                    IceLogging.Debug($"We have potentional leves that we can grab for ARR grind. Current count: {arrList.Count}", tag);
                    IceLogging.Debug($"Setting the goal leve to #{arrList[0]} just so we know which npc to go to, and also so we can select the right tab", tag);
                    Leve_Helper.LeveToGrab = arrList[0];
                    Leve_Helper.ARRLeveToGrab = 0;
                    Leve_Helper.State = LeveState.Grab_ARRLeve;

                }
                else
                {
                    IceLogging.Error("We seem to have no valid leves that we can grind... make sure that if you're on fisher, you actually have the fish on you", tag);
                    IceLogging.Error("Otherwise, make sure you've selected an npc that is within your class's level", tag);
                    SchedulerMain.DisablePlugin();
                }
            }
            else
            {
                IceLogging.Error($"We somehow found... a npc that isn't in the current config? {selectedNpc}. Please make sure one is selected in ARR grind", tag);
                SchedulerMain.DisablePlugin();
            }

            return true;
        }
    }
}
