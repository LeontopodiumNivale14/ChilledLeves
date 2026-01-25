using ChilledLeves.Utilities;
using System.Collections.Generic;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public bool SendChat { get; set; } = false;
    public int LeveAlertAmount { get; set; } = 100;
    public bool PlaySound { get; set; } = false;
    public Sounds Sounds { get; set; } = Sounds.Sound01;
    public bool ShowOverlayAlert { get; set; } = true;
    public Dictionary<ulong, string> whitelistCharacters { get; set; } = new Dictionary<ulong, string>();
    public Dictionary<ulong, string> blacklistCharacters { get; set; } = new Dictionary<ulong, string>();
    public bool whitelistFeature { get; set; } = false;
    public bool blacklistFeature { get; set; } = true;
}
