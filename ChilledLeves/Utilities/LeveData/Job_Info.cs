using ChilledLeves.Enums;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public static HashSet<uint> Assigned_LeveJobs = new() { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    public static HashSet<uint> Material_LeveJobs = new() { 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    public static HashSet<uint> Gathering_LeveJobs = new() { 2, 3};
    public static HashSet<int> LeveStatus = new() { 71041, 71045, 71055 };

    public class Class_IconInfo
    {
        public ISharedImmediateTexture ColorIcon { get; set; } = null;
    }

    public static Dictionary<LeveClass, Class_IconInfo> Job_IconDict = new()
    {
        [LeveClass.Crp] = new(),
        [LeveClass.Bsm] = new(),
        [LeveClass.Arm] = new(),
        [LeveClass.Gsm] = new(),
        [LeveClass.Ltw] = new(),
        [LeveClass.Wvr] = new(),
        [LeveClass.Alc] = new(),
        [LeveClass.Cul] = new(),
        [LeveClass.Min] = new(),
        [LeveClass.Btn] = new(),
        [LeveClass.Fsh] = new(),
    };

    public static void UpdateJobIcons()
    {
        var LeveAssignmentSheet = Svc.Data.GetExcelSheet<LeveAssignmentType>();
        for (uint i = 5; i < 13; i++)
        {
            LeveClass jobId = (LeveClass)i+3;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }

        for (uint i = 2; i < 5; i++)
        {
            LeveClass jobId = (LeveClass)i+14;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }
    }

    public static IDalamudTextureWrap GreyJobIcon(LeveClass jobId)
    {
        string greyJobIcon = jobId switch
        {
            LeveClass.Crp => "ChilledLeves.Resources.GreyscaleJobs.CRP.png",
            LeveClass.Bsm => "ChilledLeves.Resources.GreyscaleJobs.BSM.png",
            LeveClass.Arm => "ChilledLeves.Resources.GreyscaleJobs.ARM.png",
            LeveClass.Gsm => "ChilledLeves.Resources.GreyscaleJobs.GSM.png",
            LeveClass.Ltw => "ChilledLeves.Resources.GreyscaleJobs.LTW.png",
            LeveClass.Wvr => "ChilledLeves.Resources.GreyscaleJobs.WVR.png",
            LeveClass.Alc => "ChilledLeves.Resources.GreyscaleJobs.ALC.png",
            LeveClass.Cul => "ChilledLeves.Resources.GreyscaleJobs.CUL.png",
            LeveClass.Min => "ChilledLeves.Resources.GreyscaleJobs.MIN.png",
            LeveClass.Btn => "ChilledLeves.Resources.GreyscaleJobs.BTN.png",
            LeveClass.Fsh => "ChilledLeves.Resources.GreyscaleJobs.FSH.png",
            _ => "ChilledLeves.Resources.GreyscaleJobs.Default.png",
        };

        return Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), greyJobIcon).GetWrapOrEmpty();
    }
}
