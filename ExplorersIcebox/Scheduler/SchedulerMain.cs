using ExplorersIcebox.Scheduler.Tasks.GroupTask;

namespace ExplorersIcebox.Scheduler
{
    internal static unsafe class SchedulerMain
    {
        internal static bool AreWeTicking;
        internal static bool EnableTicking
        {
            get => AreWeTicking;
            private set => AreWeTicking = value;
        }
        internal static bool EnablePlugin()
        {
            EnableTicking = true;
            CurrentLoop = 0;
            UpdatedShop = false;
            return true;
        }
        internal static bool DisablePlugin()
        {
            EnableTicking = false;
            P.taskManager.Abort();
            P.visland.StopRoute();
            P.navmesh.Stop();
            return true;
        }
        private static int CurrentLoop = 0;
        internal static string CurrentProcess = "";
        internal static bool UpdatedShop = false;
        internal static bool WorkshopSelected = true;

        internal static void Tick()
        {
            if (AreWeTicking)
            {
                if (GenericThrottle)
                {
                    if (!P.taskManager.IsBusy)
                    {
                        if (C.XPGrind)
                        {
                            if (C.runInfinite || (!C.runInfinite && C.runAmount > CurrentLoop))
                            {
                                GroupIslandTask.Enqueue(GetTable(C.routeSelected));
                                P.taskManager.Enqueue(() => CurrentLoop = CurrentLoop + 1);
                            }
                            else
                            {
                                DisablePlugin();
                            }
                        }
                        else if (!C.XPGrind)
                        {
                            C.routeSelected = 0;
                            bool isAllFalse = true; // Assume all are false initially.

                            foreach (var entry in RouteDataPoint)
                            {
                                if (entry.Value.GatherRoute)
                                {
                                    C.routeSelected = entry.Key; // Update the selected route in C.
                                    isAllFalse = false; // At least one route is true.
                                    PluginLog($"Current route is now: {C.routeSelected}");
                                    break; // Exit loop as we've found the first true entry.
                                }
                            }
                            if (!isAllFalse)
                            {
                                if (RouteAmount(C.routeSelected, WorkshopSelected) != 0)
                                {
                                    GroupIslandTask.Enqueue(GetTable(C.routeSelected));
                                }
                                else if ((RouteAmount(C.routeSelected, WorkshopSelected) == 0))
                                {
                                    PluginLog($"The following route: {C.routeSelected}, was set to 0. Skipping over this route");
                                }
                                P.taskManager.Enqueue(() => RouteDataPoint[C.routeSelected].GatherRoute = false);
                            }
                            if (isAllFalse)
                            {
                                DisablePlugin();
                            }
                        }
                    }
                }
            }
        }
    }
}
