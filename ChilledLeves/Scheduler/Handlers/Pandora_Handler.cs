using ChilledLeves.Utilities.LogInfo;
using ECommons.Automation.LegacyTaskManager;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

namespace ChilledLeves.Scheduler.Handlers
{
    internal static unsafe class Pandora_Handler
    {
        internal static TaskManager taskManager = new();
        static TaskManager TaskManager => taskManager;
        private static List<int> SlotsFilled { get; set; } = new();

        // Pandora State Management [Borrowed from cosmic]
        public static bool Pandora_WasRestored = false;
        private static bool PandoraGatherState = false;
        private static bool PandoraInteractState = false;
        private static bool PandoraCordialState = false;
        private static bool PandoraAutoTurnin = false;
        internal static void Tick()
        {
            if (SchedulerMain.AreWeTicking)
            {
                //by Taurenkey https://github.com/PunishXIV/PandorasBox/blob/24a4352f5b01751767c7ca7f1d4b48369be98711/PandorasBox/Features/UI/AutoSelectTurnin.cs

                StorePandoraStates();
                if (TryGetAddonByName<AddonRequest>("Request", out var requestAddon))
                {
                    for (var i = 1; i <= requestAddon->EntryCount; i++)
                    {
                        if (SlotsFilled.Contains(requestAddon->EntryCount)) ConfirmOrAbort(requestAddon);
                        if (SlotsFilled.Contains(i)) return;
                        var val = i;
                        TaskManager.DelayNext($"ClickTurnin{val}", 10);
                        TaskManager.Enqueue(() => TryClickItem(requestAddon, val));
                    }
                }
                else
                {
                    if (SlotsFilled.Count != 0)
                        SlotsFilled.Clear();

                    if (TaskManager.IsBusy)
                        TaskManager.Abort();
                }
            }
            else
                RestorePandoraStates();
        }
        private static bool? ConfirmOrAbort(AddonRequest* addon)
        {
            if (addon->HandOverButton != null && addon->HandOverButton->IsEnabled)
            {
                new AddonMaster.Request((IntPtr)addon).HandOver();
                return true;
            }
            return false;
        }
        private static bool? TryClickItem(AddonRequest* addon, int i)
        {
            if (SlotsFilled.Contains(i)) return true;

            var contextMenu = (AtkUnitBase*)Svc.GameGui.GetAddonByName("ContextIconMenu", 1).Address;

            if (contextMenu is null || !contextMenu->IsVisible)
            {
                var slot = i - 1;
                var unk = (44 * i) + (i - 1);

                ECommons.Automation.Callback.Fire(&addon->AtkUnitBase, false, 2, slot, 0, 0);

                return false;
            }
            else
            {
                ECommons.Automation.Callback.Fire(contextMenu, false, 0, 0, 1021003, 0, 0);
                IceLogging.Debug($"Filled slot {i}", "Fill Item");
                SlotsFilled.Add(i);
                return true;
            }
        }
        public static void StorePandoraStates()
        {
            if (Pandora_WasRestored)
            {
                // This is done this way becuase pandora tends to start locking up peoples machines after a long period of time
                PandoraGatherState = P.pandora.GetFeatureEnabled("Pandora Quick Gather") ?? false;
                PandoraInteractState = P.pandora.GetFeatureEnabled("Auto-interact with Gathering Nodes") ?? false;
                PandoraCordialState = P.pandora.GetFeatureEnabled("Auto-Cordial") ?? false;
                PandoraAutoTurnin = P.pandora.GetFeatureEnabled("Auto-select Turn-ins") ?? false;

                if (PandoraGatherState)
                    P.pandora.SetFeatureEnabled("Pandora Quick Gather", false);
                if (PandoraInteractState)
                    P.pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", false);
                //if (PandoraCordialState)
                    //P.pandora.SetFeatureEnabled("Auto-Cordial", false);
                if (PandoraAutoTurnin)
                    P.pandora.SetFeatureEnabled("Auto-select Turn-ins", false);

                Pandora_WasRestored = false;
            }
        }
        public static void RestorePandoraStates()
        {
            if (!Pandora_WasRestored)
            {
                if (PandoraGatherState)
                    P.pandora.SetFeatureEnabled("Pandora Quick Gather", true);
                if (PandoraInteractState)
                    P.pandora.SetFeatureEnabled("Auto-interact with Gathering Nodes", true);
                if (PandoraCordialState)
                    P.pandora.SetFeatureEnabled("Auto-Cordial", true);
                if (PandoraAutoTurnin)
                    P.pandora.SetFeatureEnabled("Auto-select Turn-ins", true);

                Pandora_WasRestored = true;
            }
        }
    }
}
