using ChilledLeves.Enums;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.ExcelServices;
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
    private static HashSet<uint> Assigned_LeveJobs = new() { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    private static HashSet<uint> Material_LeveJobs = new() { 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    private static HashSet<uint> Gathering_LeveJobs = new() { 2, 3};

    public static readonly List<Job> LeveJobs_Material = new() { Job.CRP, Job.BSM, Job.ARM, Job.GSM, Job.WVR, Job.LTW, Job.ALC, Job.CUL, Job.FSH };
    public static readonly List<Job> LeveJobs_Gathering = new() { Job.MIN, Job.BTN };

    public static HashSet<int> LeveStatus = new() { 71041, 71045, 71055 };

    public class Class_IconInfo
    {
        public ISharedImmediateTexture ColorIcon { get; set; } = null;
    }

    public static Dictionary<Job, Class_IconInfo> Job_IconDict = new()
    {
        [Job.CRP] = new(),
        [Job.BSM] = new(),
        [Job.ARM] = new(),
        [Job.GSM] = new(),
        [Job.LTW] = new(),
        [Job.WVR] = new(),
        [Job.ALC] = new(),
        [Job.CUL] = new(),
        [Job.MIN] = new(),
        [Job.BTN] = new(),
        [Job.FSH] = new(),
    };

    public static void UpdateJobIcons()
    {
        var LeveAssignmentSheet = Svc.Data.GetExcelSheet<LeveAssignmentType>();
        for (uint i = 5; i < 13; i++)
        {
            Job jobId = (Job)i+3;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }

        for (uint i = 2; i < 5; i++)
        {
            Job jobId = (Job)i+14;
            var iconId = LeveAssignmentSheet.GetRow(i).Icon;
            if (Svc.Texture.TryGetFromGameIcon(iconId, out var iconTexture))
            {
                Job_IconDict[jobId].ColorIcon = iconTexture;
            }
        }
    }

    public static IDalamudTextureWrap GreyJobIcon(Job jobId)
    {
        string greyJobIcon = jobId switch
        {
            Job.CRP => "ChilledLeves.Resources.GreyscaleJobs.CRP.png",
            Job.BSM => "ChilledLeves.Resources.GreyscaleJobs.BSM.png",
            Job.ARM => "ChilledLeves.Resources.GreyscaleJobs.ARM.png",
            Job.GSM => "ChilledLeves.Resources.GreyscaleJobs.GSM.png",
            Job.LTW => "ChilledLeves.Resources.GreyscaleJobs.LTW.png",
            Job.WVR => "ChilledLeves.Resources.GreyscaleJobs.WVR.png",
            Job.ALC => "ChilledLeves.Resources.GreyscaleJobs.ALC.png",
            Job.CUL => "ChilledLeves.Resources.GreyscaleJobs.CUL.png",
            Job.MIN => "ChilledLeves.Resources.GreyscaleJobs.MIN.png",
            Job.BTN => "ChilledLeves.Resources.GreyscaleJobs.BTN.png",
            Job.FSH => "ChilledLeves.Resources.GreyscaleJobs.FSH.png",
            _ => "ChilledLeves.Resources.GreyscaleJobs.Default.png",
        };

        return Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), greyJobIcon).GetWrapOrEmpty();
    }
}
