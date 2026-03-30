using Dalamud.Memory;
using ECommons.ExcelServices;
using ECommons.Logging;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using Callback = ECommons.Automation.Callback;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

namespace ChilledLeves.Utilities;

// Grabbed from Lim's Battlevest here:
// https://github.com/NightmareXIV/Battlevest/blob/main/Battlevest/GuildLeve.cs#L28
// (Thank you lim for this, this saves a lot of headache)

public unsafe class GuildLeve : AddonMasterBase<AddonGuildLeve>
{
    public GuildLeve(nint addon) : base(addon)
    {
    }

    public GuildLeve(void* addon) : base(addon)
    {
    }

    public uint NumEntries => Addon->AtkValues[25].UInt;
    public string SelectedLeve => MemoryHelper.ReadSeStringNullTerminated((nint)Addon->AtkValues[1233].String.Value).GetText();
    public uint SelectedLeveId => Addon->AtkValues[1489].UInt;
    public uint JobAmount => Addon->AtkValues[6].UInt;
    public AtkComponentRadioButton* Carpenter_MinerButton => Addon->GetComponentNodeById(15)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* Blacksmith_BotanistButton => Addon->GetComponentNodeById(16)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* Armourer_FisherButton => Addon->GetComponentNodeById(17)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* GoldsmithButton => Addon->GetComponentNodeById(18)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* LeatherworkerButton => Addon->GetComponentNodeById(19)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* WeaverButton => Addon->GetComponentNodeById(20)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* AlchemistButton => Addon->GetComponentNodeById(21)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* CulinarianButton => Addon->GetComponentNodeById(22)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* FieldCraftButton => Addon->GetComponentNodeById(12)->GetAsAtkComponentRadioButton();
    public AtkComponentRadioButton* TradeCraftButton => Addon->GetComponentNodeById(13)->GetAsAtkComponentRadioButton();
    public bool SelectJob(Job job)
    {
        if (JobAmount == 3 && (uint)job < 16)
        {
            ClickButtonIfEnabled(TradeCraftButton);
            return false;
        }
        else if (JobAmount == 8 && (uint)job > 15)
        {
            ClickButtonIfEnabled(FieldCraftButton);
            return false;
        }

        if ((uint)job < 16)
        {
            ClickButtonIfEnabled(Addon->GetComponentNodeById((uint)job + 7)->GetAsAtkComponentRadioButton());
            return true;
        }
        else if ((uint)job < 19)
        {
            ClickButtonIfEnabled(Addon->GetComponentNodeById((uint)job - 1)->GetAsAtkComponentRadioButton());
            return true;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(job));
        }
    }

    public Levequest[] Levequests
    {
        get
        {
            var ret = new List<Levequest>();
            for (var i = 0; i < NumEntries; i++)
            {
                var leveName = Addon->AtkValues[626 + i * 2];
                var leveLevel = Addon->AtkValues[627 + i * 2];
                if (leveName.Type.EqualsAny(ValueType.String, ValueType.ManagedString, ValueType.String8))
                {
                    var leve = new Levequest(this, i)
                    {
                        Name = MemoryHelper.ReadSeStringNullTerminated((nint)leveName.String.Value).GetText()
                    };
                    if (leveLevel.Type.EqualsAny(ValueType.String, ValueType.ManagedString, ValueType.String8))
                    {
                        leve.Level = MemoryHelper.ReadSeStringNullTerminated((nint)leveLevel.String.Value).GetText();
                    }
                    ret.Add(leve);
                }
                else
                {
                    break;
                }
            }
            return [.. ret];
        }
    }

    public override string AddonDescription { get; }

    public class Levequest(GuildLeve master, int index)
    {
        public string Name;
        public string? Level;

        public void Select()
        {
            var quest = Svc.Data.GetExcelSheet<Leve>().FirstOrNull(x => x.Name.GetText() == Name);
            if (quest == null)
            {
                PluginLog.Error($"Failed to select levequest, requested name not found: {Name}");
            }
            else
            {
                Callback.Fire(master.Base, true, 13, index, (int)quest?.RowId);
            }
        }
    }
}
