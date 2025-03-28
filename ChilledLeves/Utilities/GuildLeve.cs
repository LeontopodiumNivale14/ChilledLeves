using Dalamud.Memory;
using ECommons.Automation;
using ECommons.Logging;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
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
