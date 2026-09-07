using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Callback = ECommons.Automation.Callback;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.AtkValueType;

namespace ChilledLeves.Utilities;

public unsafe class GuildLeveDifficulty : AddonMasterBase<AddonGuildLeveDifficulty>
{
    public GuildLeveDifficulty(nint addon) : base(addon) { }
    public GuildLeveDifficulty(void* addon) : base(addon) { }

    public override string AddonDescription { get; }

    public AtkComponentButton* Cancel => Addon->GetComponentButtonById(8);
    public AtkComponentButton* Confirm => Addon->GetComponentButtonById(7);

    public void SliderLevel(GuildLeveDifficulty master, int level)
    {
        Callback.Fire(master.Base, true, 3, level);
    }

    public void Yes() => ClickButtonIfEnabled(Confirm);
    public void No() => ClickButtonIfEnabled(Cancel);
}
