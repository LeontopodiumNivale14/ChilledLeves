using ChilledLeves.Scheduler.Handers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskGrabLeve
    {
        internal static void Enqueue(ushort leveID)
        {
            TaskInteract.Enqueue(1037263);
            P.taskManager.Enqueue(() => GrabLeve(leveID), DConfig);
            P.taskManager.EnqueueDelay(1000);
        }

        internal static unsafe bool? GrabLeve(ushort leveID)
        {
            if (IsStarted(Svc.Data.Excel.GetSheet<Leve>().GetRow((uint)leveID)) || IsReadyForTurnIn((Svc.Data.Excel.GetSheet<Leve>().GetRow((uint)leveID))))
            {
                if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
                {
                    if (EzThrottler.Throttle("Leaving Leve Person", 1000))
                    {
                        GenericHandlers.FireCallback("SelectString", true, 3);
                    }
                }
                else if (TryGetAddonByName<AtkUnitBase>("GuildLeve", out var GuildLeve) && IsAddonReady(GuildLeve))
                {
                    if (EzThrottler.Throttle("Leaving Journal Window", 1000))
                    {
                        GenericHandlers.FireCallback("GuildLeve", true, -1);
                    }
                }
                else if (PlayerNotBusy())
                {
                    return true;
                }
            }
            else
            {
                if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonReady(TalkAddon))
                {
                    if (EzThrottler.Throttle("Talk Box", 100))
                    {
                        GenericHandlers.FireCallback("Talk", true);
                    }
                }
                else if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
                {
                    if (EzThrottler.Throttle("Opening the Levequests Window", 100))
                    {
                        GenericHandlers.FireCallback("SelectString", true, 1);
                    }
                }
                else if (TryGetAddonByName<AtkUnitBase>("JournalDetail", out var JournalDetail) && IsAddonReady(JournalDetail))
                {
                    if (GetNodeText("JournalDetail", 19) != "The Mountain Steeped")
                    {
                        if (EzThrottler.Throttle("Clicking to the correct leve", 3000))
                        {
                            GenericHandlers.FireCallback("GuildLeve", true, 13, 14, 1647);
                        }
                    }
                    else if (GetNodeText("JournalDetail", 19) == "The Mountain Steeped")
                    {
                        if (EzThrottler.Throttle("Accepting the correct leve", 3000))
                        {
                            GenericHandlers.FireCallback("JournalDetail", true, 3, 1647);
                        }
                    }

                }
            }

            return false;
        }
    }
}
