using ECommons.GameHelpers;
using ECommons.Throttlers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities;

public enum Sounds : byte
{
    None = 0x00,
    Unknown = 0x01,
    Sound01 = 0x25,
    Sound02 = 0x26,
    Sound03 = 0x27,
    Sound04 = 0x28,
    Sound05 = 0x29,
    Sound06 = 0x2A,
    Sound07 = 0x2B,
    Sound08 = 0x2C,
    Sound09 = 0x2D,
    Sound10 = 0x2E,
    Sound11 = 0x2F,
    Sound12 = 0x30,
    Sound13 = 0x31,
    Sound14 = 0x32,
    Sound15 = 0x33,
    Sound16 = 0x34,
}

internal static class SoundAlert
{
    public static string ToName(this Sounds value)
    {
        return value switch
        {
            Sounds.None => "None",
            Sounds.Sound01 => "Sound Effect 1",
            Sounds.Sound02 => "Sound Effect 2",
            Sounds.Sound03 => "Sound Effect 3",
            Sounds.Sound04 => "Sound Effect 4",
            Sounds.Sound05 => "Sound Effect 5",
            Sounds.Sound06 => "Sound Effect 6",
            Sounds.Sound07 => "Sound Effect 7",
            Sounds.Sound08 => "Sound Effect 8",
            Sounds.Sound09 => "Sound Effect 9",
            Sounds.Sound10 => "Sound Effect 10",
            Sounds.Sound11 => "Sound Effect 11",
            Sounds.Sound12 => "Sound Effect 12",
            Sounds.Sound13 => "Sound Effect 13",
            Sounds.Sound14 => "Sound Effect 14",
            Sounds.Sound15 => "Sound Effect 15",
            Sounds.Sound16 => "Sound Effect 16",
            _ => "Menu Select",
        };
    }

    public static Sounds SoundEffect = C.Sounds;
    public static ulong CID => Svc.ClientState.LocalContentId;
    public static readonly ulong DefaultCID = 0000000000000000;
    public static ulong CurrentId = 0000000000000000;
    public static bool refreshOverlay = false;

    public static void Tick()
    {
        if ((CID != CurrentId || CID == DefaultCID) && C.ShowOverlayAlert)
        {
            CurrentId = DefaultCID;
            refreshOverlay = true;
        }

        if (Utils.Allowances >= C.LeveAlertAmount && !Player.IsInDuty && CurrentId != CID && CID != DefaultCID)
        {
            CurrentId = CID;
            if (EzThrottler.Throttle("PlaySoundEffect", 100))
            {
                if (refreshOverlay)
                {
                    P.alertUi.IsOpen = true;
                }

                if (C.SendChat)
                {
                    ECommons.ChatMethods.ChatPrinter.Orange($"[Chilled Leves] Leve's are at: {Allowances}");
                }
                if (C.PlaySound)
                {
                    UIGlobals.PlaySoundEffect((uint)SoundEffect);
                }
            }
        }
    }
}
