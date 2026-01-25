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

    public class Class_IconInfo
    {
        public ISharedImmediateTexture ColorIcon { get; set; } = null;
    }

    public static Dictionary<uint, Class_IconInfo> Job_IconDict = new()
    {
        [8] = new(),
        [9] = new(),
        [10] = new(),
        [11] = new(),
        [12] = new(),
        [13] = new(),
        [14] = new(),
        [15] = new(),
        [16] = new(),
        [17] = new(),
        [18] = new(),
    };

    public static void UpdateJobIcons()
    {
        var LeveAssignmentSheet = Svc.Data.GetExcelSheet<LeveAssignmentType>();
        for (uint i = 5; i < 13; i++)
        {
            uint jobId = i+3;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }

        for (uint i = 2; i < 5; i++)
        {
            uint jobId = i+14;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }
    }

    public static IDalamudTextureWrap GreyJobIcon(uint jobId)
    {
        string greyJobIcon = jobId switch
        {
            8 => "ChilledLeves.Resources.GreyscaleJobs.CRP.png",
            9 => "ChilledLeves.Resources.GreyscaleJobs.BSM.png",
            10 => "ChilledLeves.Resources.GreyscaleJobs.ARM.png",
            11 => "ChilledLeves.Resources.GreyscaleJobs.GSM.png",
            12 => "ChilledLeves.Resources.GreyscaleJobs.LTW.png",
            13 => "ChilledLeves.Resources.GreyscaleJobs.WVR.png",
            14 => "ChilledLeves.Resources.GreyscaleJobs.ALC.png",
            15 => "ChilledLeves.Resources.GreyscaleJobs.CUL.png",
            16 => "ChilledLeves.Resources.GreyscaleJobs.MIN.png",
            17 => "ChilledLeves.Resources.GreyscaleJobs.BTN.png",
            18 => "ChilledLeves.Resources.GreyscaleJobs.FSH.png",
            _ => "ChilledLeves.Resources.GreyscaleJobs.Default.png",
        };

        return Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), greyJobIcon).GetWrapOrEmpty();
    }
}
