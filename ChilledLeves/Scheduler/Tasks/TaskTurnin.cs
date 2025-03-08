using ChilledLeves.Scheduler.Handlers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace ChilledLeves.Scheduler.Tasks
{
    internal static class TaskTurnin
    {
        internal static unsafe void Enqueue(string QuestName, uint leveID)
        {
            P.taskManager.Enqueue(() => Turnin(QuestName, leveID), configuration: DConfig);
            P.taskManager.Enqueue(() => !IsAccepted(leveID));
            P.taskManager.Enqueue(() => PlayerNotBusy());
        }

        internal static unsafe bool? Turnin(string QuestName, uint leveID)
        {
            if (!IsAccepted(leveID))
            {
                return true;
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectIconString", out var QuestAddon) && IsAddonReady(QuestAddon))
            {
                int callback = GetCallback(QuestName);
                if (EzThrottler.Throttle("Selecting Correct Quest Turnin", 2000))
                {
                    PluginVerbos("Selecting the turnin quest");
                    GenericHandlers.FireCallback("SelectIconString", true, callback);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("Talk", out var TalkAddon) && IsAddonReady(TalkAddon))
            {
                if (EzThrottler.Throttle("Talk Box", 100))
                {
                    PluginVerbos("Closing the Talk box");
                    GenericHandlers.FireCallback("Talk", true);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectYesno", out var SelectYes) && IsAddonReady(SelectYes))
            {
                if (EzThrottler.Throttle("SelectYes to HQ", 100))
                {
                    PluginVerbos("Re-confirming that you want to turn in HQ");
                    GenericHandlers.FireCallback("SelectYesno", true, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("JournalResult", out var JournalAccept) && IsAddonReady(JournalAccept))
            {
                if (EzThrottler.Throttle("Accepting the journal", 100))
                {
                    PluginVerbos("Accepting the journal results");
                    GenericHandlers.FireCallback("JournalResult", true, 0, 0);
                }
            }
            else if (TryGetAddonByName<AtkUnitBase>("SelectString", out var SelectString) && IsAddonReady(SelectString))
            {
                if (EzThrottler.Throttle("Accepting Multi-Turnin", 200))
                {
                    PluginVerbos("Saying yes to turning in multiple items");
                    GenericHandlers.FireCallback("SelectString", true, 0);
                }
            }

            return false;
        }
    }
}

