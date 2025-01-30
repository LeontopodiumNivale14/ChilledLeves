using ECommons.Automation;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ExplorersIcebox.Scheduler.Handers
{
    internal class GenericHandlers
    {
        internal static bool? Throttle(string name, int ms)
        {
            return EzThrottler.Throttle(name, ms);
        }

        internal static bool? WaitFor(string name)
        {
            return EzThrottler.Check(name);
        }
        internal static unsafe bool? FireCallback(string AddonName, bool visibilty, params int[] callback_fires)
        {
            if (GenericHelpers.TryGetAddonByName<AtkUnitBase>(AddonName, out var addon) && GenericHelpers.IsAddonReady(addon))
            {
                Callback.Fire(addon, visibilty, callback_fires.Cast<object>().ToArray());
                return true;
            }
            return false;
        }
    }
}
