namespace ChilledLeves.Scheduler
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

        internal static string CurrentProcess = "";

        // dummy values so I can get the logic in 
        private static uint RunAmount = 0;
        private static uint ClassLevel = 0;
        private static uint TurninZone = 0;
        private static uint CurrentZone = 0;

        internal static void Tick()
        {
            if (AreWeTicking)
            {
                if (GenericThrottle)
                {
                    if (!P.taskManager.IsBusy)
                    {
                        uint leve = 0;
                        bool noLeves = false;
                        foreach (var entry in C.workList)
                        {
                            if (entry.InputValue != 0)
                            {
                                PluginLog($"Worklist has found that: {entry.LeveID} has an amount that isn't set to 0. Starting to work on this turnin process");
                                leve = entry.LeveID;
                                break;
                            }
                            else
                                noLeves = true;
                        }
                        if (C.workList.Count > 0 && !noLeves)
                        {
                            if (LeveDict[leve].Level >= ClassLevel)
                            {
                                // Task to Teleport to Zone
                                // Task to use aetherytes
                                // Task to path to leve vendor
                                if (TurninZone != CurrentZone)
                                {
                                    // Tp to 2nd zone
                                    // If mounting is allowed (which should be) mount 
                                    // Path to turnin vendor (probably do ground movement... think about this later
                                }
                                // Turn into vendor
                                // Auto turnin
                                // Add a check for NQ/HQ promps
                                // Wait for player to not be busy/interacting with NPC's
                                // -1 from the workshop run amount
                            }
                            else
                            {
                                 // Need to add a prompt to say that you're not the required level for this leve and skips it
                            }
                        }
                        else
                        {
                            PluginLog("No workshop items are currently set. And yet somehow you managed to run this... IMPRESSIVE. But stopping the system");
                            DisablePlugin();
                        }
                    }
                }
            }
        }
    }
}
