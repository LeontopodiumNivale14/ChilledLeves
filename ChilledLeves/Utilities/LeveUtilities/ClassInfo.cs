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
            public string GreyPath { get; set; }
            public uint ColorIconId { get; set; }
        }

        public static Dictionary<uint, ClassInfo> JobInfo = new()
        {
            [8] = new ClassInfo
            {
                Name = "CRP",
                GreyPath = "ChilledLeves.Resources.CRP.png",
                ColorIconId = 62502,
            },
            [9] = new ClassInfo
            {
                Name = "BSM",
                GreyPath = "ChilledLeves.Images.BSM.png",
                ColorIconId = 62503,
            },
            [10] = new ClassInfo
            {
                Name = "ARM",
                GreyPath = "ChilledLeves.Images.ARM.png",
                ColorIconId = 62504,
            },
            [11] = new ClassInfo
            {
                Name = "GSM",
                GreyPath = "ChilledLeves.Images.GSM.png",
                ColorIconId = 62505,
            },
            [12] = new ClassInfo
            {
                Name = "LTW",
                GreyPath = "ChilledLeves.Images.LTW.png",
                ColorIconId = 62506,
            },
            [13] = new ClassInfo
            {
                Name = "WVR",
                GreyPath = "ChilledLeves.Images.WVR.png",
                ColorIconId = 62507,
            },
            [14] = new ClassInfo
            {
                Name = "ALC",
                GreyPath = "ChilledLeves.Images.ALC.png",
                ColorIconId = 62508,
            },
            [15] = new ClassInfo
            {
                Name = "CUL",
                GreyPath = "ChilledLeves.Images.CUL.png",
                ColorIconId = 62509,
            },
            [16] = new ClassInfo
            {
                Name = "MIN",
                GreyPath = "ChilledLeves.Images.MIN.png",
                ColorIconId = 62510,
            },
            [17] = new ClassInfo
            {
                Name = "BTN",
                GreyPath = "ChilledLeves.Images.BTN.png",
                ColorIconId = 62511,
            },
            [18] = new ClassInfo
            {
                Name = "FSH",
                GreyPath = "ChilledLeves.Images.FSH.png",
                ColorIconId = 62512,
            },
        };
    }
}
