using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ChilledLeves.Config_Files;

public partial class Config
{
    public bool UseMount { get; set; } = true;
    public int Mount_MinDistance { get; set; } = 15;
    public int Mount_DismountDistance { get; set; } = 7;
    public bool OptionalFly { get; set; } = true;
    public int Fly_MinDistance { get; set; } = 20;
    public uint MountId { get; set; } = 0;
    public string MountName { get; set; } = string.Empty;

    public float GatherFanSectionSize { get; set; } = 45f;
}
