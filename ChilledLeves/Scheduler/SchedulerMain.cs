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
            P.navmesh.Stop();
            P.taskManager.Abort();
           
            return true;
        }

        internal static string CurrentProcess = "";
        internal static bool KeepLeves = false;
        internal static bool GatheringMode = false;
        internal static bool WorkListMode = false;
        private static int MinMountDistance = 25;
        private static float InteractDistance = 6.8f;

        internal static void Tick()
        {
            if (AreWeTicking)
            {

            }
        }
    }
}
