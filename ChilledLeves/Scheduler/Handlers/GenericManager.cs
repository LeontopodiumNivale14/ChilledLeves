using ECommons.Automation.LegacyTaskManager;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Collections.Generic;

namespace ChilledLeves.Scheduler.Handlers
{
    internal static unsafe class GenericManager
    {
        internal static TaskManager taskManager = new();
        static TaskManager TaskManager => taskManager;
        private static List<int> SlotsFilled { get; set; } = new();
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
                Svc.Log.Debug($"Filled slot {i}");
                SlotsFilled.Add(i);
                return true;
            }
        }

        internal static void Tick()
        {
            if (SchedulerMain.AreWeTicking)
            {
                //by Taurenkey https://github.com/PunishXIV/PandorasBox/blob/24a4352f5b01751767c7ca7f1d4b48369be98711/PandorasBox/Features/UI/AutoSelectTurnin.cs

                var featureEnabled = (P.pandora.GetFeatureEnabled("Auto-select Turn-ins") ?? false);
                var configEnabled = (P.pandora.GetConfigEnabled("Auto-select Turn-ins", "AutoSelect") ?? false);

                var isenabled = featureEnabled && configEnabled;

                if (!isenabled)
                {
                    if (featureEnabled && !configEnabled)
                    {
                        if (EzThrottler.Throttle("Enabling AutoSelect", 1000))
                        {
                            P.pandora.PauseFeature("Auto-select Turn-ins", 1100);
                        }
                    }

                    if (TryGetAddonByName<AddonRequest>("Request", out var addon3))
                    {
                        for (var i = 1; i <= addon3->EntryCount; i++)
                        {
                            if (SlotsFilled.Contains(addon3->EntryCount)) ConfirmOrAbort(addon3);
                            if (SlotsFilled.Contains(i)) return;
                            var val = i;
                            TaskManager.DelayNext($"ClickTurnin{val}", 10);
                            TaskManager.Enqueue(() => TryClickItem(addon3, val));
                        }
                    }
                    else
                    {
                        SlotsFilled.Clear();
                        TaskManager.Abort();
                    }
                }
            }

        }
    }
}
