using ChilledLeves.Scheduler.Handlers;
using ECommons.ExcelServices;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ChilledLeves.Scheduler.Tasks.Old_Task
{
    internal static class TaskClassChange
    {
        public static void Enqueue(Job jobID)
        {
            P.taskManager.Enqueue(() => ChangeClass(jobID));
        }

        internal unsafe static bool? ChangeClass(Job jobID)
        {
            if (GetClassJobId() == (uint)jobID)
                return true;

            if (EzThrottler.Throttle("Equipping class"))
            {
                var gearsets = RaptureGearsetModule.Instance();
                foreach (ref var gs in gearsets->Entries)
                {
                    if (!RaptureGearsetModule.Instance()->IsValidGearset(gs.Id)) continue;
                    if ((Job)gs.ClassJob == jobID)
                    {
                        if (gs.Flags.HasFlag(RaptureGearsetModule.GearsetFlag.MainHandMissing))
                        {
                            if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var selectYesno) && IsAddonActive("SelectYesno"))
                            {
                                GenericHandlers.FireCallback("SelectYesno", true, 0);
                            }
                            else
                            {
                                gearsets->EquipGearset(gs.Id);
                            }
                        }
                        else
                        {
                            gearsets->EquipGearset(gs.Id);
                        }
                    }
                }
            }

            return false;
        }
    }
}
