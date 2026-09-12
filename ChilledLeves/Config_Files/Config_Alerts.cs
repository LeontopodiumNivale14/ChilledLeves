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

    public class ClassInformation
    {
        public string Name { get; set; }
        public string World { get; set; }
        public bool AllowNotification { get; set; } = false;
        public int LastKnownAllowance { get; set; }
        public DateTime Time_LastObserved { get; set; } = DateTime.MinValue;
        public DateTime Time_NextTickAt { get; set; } = DateTime.MinValue;
        public bool Blacklisted { get; set; } = false;
    }

    public Dictionary<ulong, ClassInformation> CharacterInfo { get; set; } = new();
    public List<ulong> Character_Order { get; set; } = new();
}
