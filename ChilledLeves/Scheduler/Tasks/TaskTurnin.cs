using ChilledLeves.Scheduler.Handers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTurnin
    {
        internal static unsafe void Enqueue(string QuestName)
        {
            P.taskManager.Enqueue(() => Turnin(QuestName));
        }

        internal static unsafe bool? Turnin(string QuestName)
        {
            if (TryGetAddonByName<AtkUnitBase>("SelectIconString", out var QuestAddon) && IsAddonReady(QuestAddon))
            {
                int callback = GetCallback(QuestName);
                if (EzThrottler.Throttle("Selecting Correct Quest Turnin", 2000))
                {
                    GenericHandlers.FireCallback("SelectIconString", true, callback);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonReady(TalkAddon))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    GenericHandlers.FireCallback("Talk", true);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var SelectYes) && IsAddonReady(SelectYes))
            {
                if (EzThrottler.Throttle("SelectYes to HQ", 100))
                {
                    GenericHandlers.FireCallback("SelectYesno", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("JournalResult", out var JournalAccept) && IsAddonReady(JournalAccept))
            {
                if (EzThrottler.Throttle("Accepting the journal", 100))
                {
                    GenericHandlers.FireCallback("JournalResult", true, 0, 0);
                    return true;
                }
            }

            return false;
        }
    }
}
