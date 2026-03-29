using ChilledLeves.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Scheduler
{
    internal class Leve_Helper
    {
        internal static uint LeveToGrab = 0;

        internal static Leve_State State = Leve_State.Idle;
        internal static Leve_Mode SelectedMode = Leve_Mode.Standard;
    }
}
