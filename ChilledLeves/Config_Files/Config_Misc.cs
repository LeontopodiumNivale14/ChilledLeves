using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public bool SafetyDelay_LeveGrab { get; set; } = true;
    public int LeveGrab_DelayTimer { get; set; } = 500;
}