using ECommons.EzSharedDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Handlers
{
    internal static class TextAdvancedManager
    {
        private static bool WasChanged = false;
        internal static void Tick()
        {
            if (WasChanged)
            {
                if (!SchedulerMain.AreWeTicking)
                {
                    WasChanged = false;
                    UnlockTA();
                    PluginDebug($"TextAdvance unlocked");
                }
            }
            else
            {
                if (SchedulerMain.AreWeTicking)
                {
                    WasChanged = true;
                    LockTA();
                    PluginDebug($"TextAdvance locked");
                }
            }
        }
        internal static void LockTA()
        {
            if (EzSharedData.TryGet<HashSet<string>>("TextAdvance.StopRequests", out var data))
            {
                data.Add(P.Name);
            }
        }

        internal static void UnlockTA()
        {
            if (EzSharedData.TryGet<HashSet<string>>("TextAdvance.StopRequests", out var data))
            {
                data.Remove(P.Name);
            }
        }
    }
}

