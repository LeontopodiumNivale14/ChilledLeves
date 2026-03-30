using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using System.Collections.Generic;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Scheduler.Tasks
{
    internal class Task_GrabLeve
    {
        public static void Enqueue()
        {
            P.taskManager.EnqueueMulti
            (
                new(() => GrabLeve(), "Grabbing the leve from the vendor"),
                new(() => CheckOtherLeves(), "Checking for multi leve grab")
            );
        }

        public static bool GrabLeve()
        {
            string tag = "Task: Grab Leve";
            
            if (Utils.Leve_IsAccepted(Leve_Helper.LeveToGrab))
            {
                IceLogging.Debug("We've accepted the leve, continuing onto checking for multi leves", tag);
                return true;
            }

            if (GenericHelpers.TryGetAddonMaster<GuildLeve>("GuildLeve", out var guildLeve) && guildLeve.IsAddonReady)
            {
                if (Leve_Helper.LeveToGrab == guildLeve.SelectedLeveId)
                {
                    if (EzThrottler.Throttle("Accepting Leve", 1000))
                        GenericHandlers.FireCallback("JournalDetail", true, 3, (int)Leve_Helper.LeveToGrab);

                    return false;
                }

                foreach (var leve in guildLeve.Levequests)
                {
                    var selectedLeve = LeveInfo.Leve_SheetInfo.Where(x => x.Value.LeveName == leve.Name).FirstOrNull();
                    if (selectedLeve != null)
                    {
                        var selectedJob = selectedLeve.Value.Value.Job;
                        var goalLeve = LeveInfo.Leve_SheetInfo[Leve_Helper.LeveToGrab];
                        var goalJob = goalLeve.Job;
                        var goalName = goalLeve.LeveName;

                        if (selectedJob != goalJob)
                        {
                            guildLeve.SelectJob(goalJob);
                        }
                        else if (leve.Name == goalName)
                        {
                            if (EzThrottler.Throttle("Leve_CorrectJob", 1000))
                            {
                                IceLogging.Verbose($"Selecting leve: {leve.Name}", tag);
                                leve.Select();
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool CheckOtherLeves()
        {
            string tag = "Task_GrabLeve: Check Multi State";

            if (C.GrabMulti)
            {

            }
            else
            {
                IceLogging.Verbose("We were told that Grab Multi-Leves was disabled, so we're going to stop", tag);
                return true;
            }

            return false;
        }
    }
}
