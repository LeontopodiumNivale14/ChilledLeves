using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class Task_CheckLeves
    {
        public static void Enqueue()
        {
            
        }

        private static bool Check_CurrentLeves()
        {
            string tag = "Check Active Leves";

            Leve_Helper.LeveToGrab = 0;

            var currentLeves = Utils.Leve_ActiveIds();
            foreach (var leve in currentLeves)
            {
                if (C.LeveOrder.Contains(leve))
                {
                    if (LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo))
                    {
                        if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                        {
                            var materialInfo = sheetInfo.MaterialInfo;
                            var neededAmount = Utils.Leve_RequiredAmount(materialInfo);

                            if (Utils.GetItemCount(materialInfo.Item_Id) >= neededAmount)
                            {
                                IceLogging.Debug($"Found a leve that we have enough for a turnin. Going to go do so {leve}", tag);
                                Leve_Helper.LeveToGrab = leve;
                                Leve_Helper.State = Leve_State.Turnin_Leve;
                                return true;
                            }
                        }
                        else if (LeveInfo.LeveJobs_Gathering.Contains(sheetInfo.Job))
                        {
                            var currentSeq = Utils.Leve_Sequence(leve);

                            if (currentSeq is 1 or 3)
                            {
                                IceLogging.Debug($"Leve [{leve}] still needs to be completed/redone. So going to go do so", tag);
                                Leve_Helper.LeveToGrab = leve;
                                Leve_Helper.State = Leve_State.Start_GatheringLeve;
                            }
                            else if (currentSeq is 255)
                            {
                                IceLogging.Debug($"Leve [{leve}] is completed/ready to be turned in. Going to do so", tag);
                                Leve_Helper.LeveToGrab = leve;
                                Leve_Helper.State = Leve_State.Turnin_Leve;
                                return true;
                            }
                        }
                    }
                }
            }

            if (Leve_Helper.LeveToGrab == 0)
            {
                IceLogging.Debug("No active leves were found that were in our list, so we're going to instead find one to complete", tag);
                P.taskManager.Enqueue(() => Check_StandardLeves(), "Checking Standard Leves");
            }

            return false;
        }

        private static bool? Check_StandardLeves()
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
                        IceLogging.Verbose($"Found a leve that isn't at a count of 0! Checking to see what requirements are needed", tag);
                        if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                        {
                            var materialInfo = sheetInfo.MaterialInfo;
                            var neededAmount = Utils.Leve_RequiredAmount(materialInfo);

                            if (Utils.GetItemCount(materialInfo.Item_Id) >= neededAmount)
                            {
                                IceLogging.Debug($"Found a leve that we have enough for a turnin. Going to go do grab {leve}", tag);
                                Leve_Helper.LeveToGrab = leve;
                                break;
                            }
                        }
                        else
                        {
                            IceLogging.Verbose($"We're doing a gathering mission! Hopefully it's one where we can actually grab it (Post ARR)", tag);
                            IceLogging.Debug($"Goal: LeveID {leve}", tag);

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

            if (Leve_Helper.LeveToGrab == 0)
            {
                IceLogging.Debug("We've found no leves that we're able to complete, so we're just going to stop the process", tag);
                Leve_Helper.State = Leve_State.Idle;
                return true;
            }
            else
            {
                IceLogging.Verbose("Swapping to traveling to grab said leve", tag);
                Leve_Helper.State = Leve_State.Travel;
            }

            return false;
        }
    }
}
