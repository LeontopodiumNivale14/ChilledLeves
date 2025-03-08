using ECommons.EzSharedDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Scheduler.Handlers
{
    internal static class YesAlreadyManager
    {
        private static bool WasChanged = false;
        internal static void Tick()
        {
            if (WasChanged)
            {
                if (!SchedulerMain.AreWeTicking)
                {
                    WasChanged = false;
                    Unlock();
                    PluginDebug($"YesAlready unlocked");
                }
            }
            else
            {
                if (SchedulerMain.AreWeTicking)
                {
                    WasChanged = true;
                    Lock();
                    PluginDebug($"YesAlready locked");
                }
            }
        }
        internal static void Lock()
        {
            if (EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out var data))
            {
                data.Add(P.Name);
            }
        }

        internal static void Unlock()
        {
            if (EzSharedData.TryGet<HashSet<string>>("YesAlready.StopRequests", out var data))
            {
                data.Remove(P.Name);
            }
        }
    }
}
