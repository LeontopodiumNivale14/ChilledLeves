using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.LogInfo;
using System.Collections.Generic;

namespace ChilledLeves.Scheduler
{
    internal class Leve_Helper
    {
        internal static uint LeveToGrab = 0;
        internal static uint ARRLeveToGrab = 0;

        internal static LeveState State = LeveState.Idle;
        internal static ModeSelection SelectedMode = ModeSelection.Standard;

        internal static bool IsIdle => State == LeveState.Idle;
        internal static List<uint> ValidARRLeves = new();
    }
}
