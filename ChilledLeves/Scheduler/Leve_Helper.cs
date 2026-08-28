using ChilledLeves.Enums;

namespace ChilledLeves.Scheduler
{
    internal class Leve_Helper
    {
        internal static uint LeveToGrab = 0;

        internal static LeveState State = LeveState.Idle;
        internal static ModeSelection SelectedMode = ModeSelection.Standard;

        internal static bool IsIdle => State == LeveState.Idle;
    }
}
