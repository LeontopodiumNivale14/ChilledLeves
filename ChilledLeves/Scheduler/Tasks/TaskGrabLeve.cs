using ChilledLeves.Scheduler.Handlers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskGrabLeve
    {
        internal static void Enqueue(uint leveID, uint npcID)
        {
            TaskInteract.Enqueue(npcID);
            P.taskManager.Enqueue(() => GrabLeve((ushort)leveID, npcID), DConfig);
            P.taskManager.Enqueue(() => LeaveLeveVendor(npcID), DConfig);
            P.taskManager.Enqueue(() => PlayerNotBusy(), DConfig);
        }

        internal static unsafe bool? GrabLeve(ushort leveID, uint npcID)
        {
            var LeveName = CrafterLeves[leveID].LeveName;
            var craftButton = LeveNPCDict[npcID].CrafterButton;

            if (IsAccepted(leveID))
            {
                return true;
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonActive("Talk"))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    GenericHandlers.FireCallback("Talk", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
            {
                if (EzThrottler.Throttle("Opening the Levequests Window", 100))
                {
                    GenericHandlers.FireCallback("SelectString", true, craftButton);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("JournalDetail", out var JournalDetail) && IsAddonReady(JournalDetail))
            {
                if (GetNodeText("JournalDetail", 19) != LeveName)
                {
                    if (EzThrottler.Throttle("Clicking to the correct leve", 2000))
                    {
                        GenericHandlers.FireCallback("GuildLeve", true, 13, 14, leveID);
                    }
                }
                else if (GetNodeText("JournalDetail", 19) == LeveName)
                {
                    if (EzThrottler.Throttle("Accepting the correct leve", 2000))
                    {
                        GenericHandlers.FireCallback("JournalDetail", true, 3, leveID);
                    }
                }
            }

            return false;
        }

        internal static unsafe bool? LeaveLeveVendor(uint npcID)
        {
            var leaveButton = LeveNPCDict[npcID].LeaveButton;

            if (TryGetAddonByName<AtkUnitBase>("SelectString", out var LeveWindow) && IsAddonReady(LeveWindow))
            {
                if (EzThrottler.Throttle("Leaving Leve Person", 500))
                {
                    GenericHandlers.FireCallback("SelectString", true, leaveButton);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("GuildLeve", out var GuildLeve) && IsAddonReady(GuildLeve))
            {
                if (EzThrottler.Throttle("Leaving Journal Window", 500))
                {
                    GenericHandlers.FireCallback("GuildLeve", true, -1);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonActive("Talk"))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    GenericHandlers.FireCallback("Talk", true, 0);
                }
            }
            else if (PlayerNotBusy())
            {
                return true;
            }

            return false;
        }
    }
}
