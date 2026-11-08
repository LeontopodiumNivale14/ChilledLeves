using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public bool SafetyDelay_LeveGrab { get; set; } = true;
    public int LeveGrab_DelayTimer { get; set; } = 500;
    public bool Delay_Interaction { get; set; } = false;
    public int Interaction_DelayTimer { get; set; } = 500;

    public bool InteractionDelay()
    {
        if (Delay_Interaction && !EzThrottler.Throttle("Interaction Delay", Interaction_DelayTimer))
            return true;

        else
            return false;
    }
}