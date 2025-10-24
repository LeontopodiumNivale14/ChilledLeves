using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities.LeveUtilities
{
    public static partial class LeveInfo
    {
        public static HashSet<uint> CrafterLeveSheetJobs = new() { 8, 9, 10, 11, 12, 13, 14, 15, 18 };
        public static HashSet<uint> GatheringLeveSheetJobs = new() { 17, 18 };
        public static HashSet<int> SupportedPriorityJobs = new() { 16, 17, 18 };

        // - - - - - - - - - 
        // General Info
        // - - - - - - - - -

        public class ClassInfo
        {
            public string Name { get; set; }
            public uint GreyIconId { get; set; }
            public string GreyPath { get; set; }
            public uint ColorIconId { get; set; }
            public IDalamudTextureWrap GreyIcon { get; set; }
            public ISharedImmediateTexture ColorIcon { get; set; }
        }

        public static Dictionary<uint, ClassInfo> JobInfo = new()
        {
            [8] = new ClassInfo
            {
                Name = "CRP",
                GreyIconId = 91031,
                GreyPath = "ChilledLeves.Images.CRP.png",
                ColorIconId = 62502,
                GreyIcon = null,
                ColorIcon = null,
            },
            [9] = new ClassInfo
            {
                Name = "BSM",
                GreyPath = "ChilledLeves.Images.BSM.png",
                GreyIconId = 91032,
                ColorIconId = 62503,
                GreyIcon = null,
                ColorIcon = null,
            },
            [10] = new ClassInfo
            {
                Name = "ARM",
                GreyPath = "ChilledLeves.Images.ARM.png",
                GreyIconId = 91033,
                ColorIconId = 62504,
                GreyIcon = null,
                ColorIcon = null,
            },
            [11] = new ClassInfo
            {
                Name = "GSM",
                GreyPath = "ChilledLeves.Images.GSM.png",
                GreyIconId = 91034,
                ColorIconId = 62505,
                GreyIcon = null,
                ColorIcon = null,
            },
            [12] = new ClassInfo
            {
                Name = "LTW",
                GreyPath = "ChilledLeves.Images.LTW.png",
                GreyIconId = 91035,
                ColorIconId = 62506,
                GreyIcon = null,
                ColorIcon = null,
            },
            [13] = new ClassInfo
            {
                Name = "WVR",
                GreyPath = "ChilledLeves.Images.WVR.png",
                GreyIconId = 91036,
                ColorIconId = 62507,
                GreyIcon = null,
                ColorIcon = null,
            },
            [14] = new ClassInfo
            {
                Name = "ALC",
                GreyPath = "ChilledLeves.Images.ALC.png",
                GreyIconId = 91037,
                ColorIconId = 62508,
                GreyIcon = null,
                ColorIcon = null,
            },
            [15] = new ClassInfo
            {
                Name = "CUL",
                GreyPath = "ChilledLeves.Images.CUL.png",
                GreyIconId = 91038,
                ColorIconId = 62509,
                GreyIcon = null,
                ColorIcon = null,
            },
            [16] = new ClassInfo
            {
                Name = "MIN",
                GreyPath = "ChilledLeves.Images.MIN.png",
                GreyIconId = 91039,
                ColorIconId = 62510,
                GreyIcon = null,
                ColorIcon = null,
            },
            [17] = new ClassInfo
            {
                Name = "BTN",
                GreyPath = "ChilledLeves.Images.BTN.png",
                GreyIconId = 91040,
                ColorIconId = 62511,
                GreyIcon = null,
                ColorIcon = null,
            },
            [18] = new ClassInfo
            {
                Name = "FSH",
                GreyPath = "ChilledLeves.Images.FSH.png",
                GreyIconId = 91041,
                ColorIconId = 62512,
                GreyIcon = null,
                ColorIcon = null,
                
            },
        };

        public static void UpdateIcons()
        {
            foreach (var jobId in JobInfo)
            {
                var jobInfo = jobId.Value;
                if (Svc.Texture.TryGetFromGameIcon(jobInfo.ColorIconId, out var colorIcon))
                {
                    jobInfo.ColorIcon = colorIcon;
                }
                jobInfo.GreyIcon = Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), jobInfo.GreyPath).GetWrapOrEmpty();
            }
        }
    }
}
