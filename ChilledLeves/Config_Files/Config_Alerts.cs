using ChilledLeves.Enums;
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

    public class ClassInformation
    {
        public string Name { get; set; }
        public string World { get; set; }
        public bool AllowNotification { get; set; } = false;
        public int LastKnownAllowance { get; set; }
        public DateTime Time_LastObserved { get; set; } = DateTime.MinValue;
        public DateTime Time_NextTickAt { get; set; } = DateTime.MinValue;
    }

    public Dictionary<ulong, ClassInformation> CharacterInfo { get; set; } = new();
}
