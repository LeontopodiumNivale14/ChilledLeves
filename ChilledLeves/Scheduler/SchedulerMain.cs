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
            P.navmesh.PathStop();
            P.taskManager.Abort();
           
            return true;
        }

        internal static void Tick()
        {
            if (AreWeTicking)
            {

            }
        }
    }
}
