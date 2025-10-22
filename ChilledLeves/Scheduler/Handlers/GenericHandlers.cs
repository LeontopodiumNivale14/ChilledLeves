using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;

namespace ChilledLeves.Scheduler.Handlers
{
    internal class GenericHandlers
    {
        internal static unsafe bool? FireCallback(string AddonName, bool visibilty, params int[] callback_fires)
        {
            if (TryGetAddonByName<AtkUnitBase>(AddonName, out var addon) && IsAddonReady(addon))
            {
                Callback.Fire(addon, visibilty, callback_fires.Cast<object>().ToArray());
                return true;
            }
            return false;
        }
    }
}

