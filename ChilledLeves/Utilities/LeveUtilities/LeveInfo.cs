using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Application.Network.WorkDefinitions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Utilities.LeveUtilities
{
    public static partial class LeveInfo
    {
        // Unsure if I need these two for the largescales or not... 
        public static HashSet<uint> IshguardCrafterLeves = new() 
        { 
            // CRP
            911, 912, 913, 917, 918, 919, 923, 924, 925, 929, 930, 931, 935, 936, 937, 
            // LTW
            941, 942, 943, 947, 948, 949, 953, 954, 955, 959, 960, 961, 965, 966, 967, 
            // BSM
            971, 972, 973, 977, 978, 979, 983, 984, 985, 989, 990, 991, 995, 996, 997, 
            // ARM
            1001, 1002, 1003, 1007, 1008, 1009, 1013, 1014, 1015, 1019, 1020, 1021, 1025, 1026, 1027, 
            // CUL
            1031, 1032, 1033, 1037, 1038, 1039, 1043, 1044, 1045, 1049, 1050, 1051, 1055, 1056, 1057, 
            // ALC
            1061, 1062, 1063, 1067, 1068, 1069, 1073, 1074, 1075, 1079, 1080, 1081, 1085, 1086, 1087, 
            // WVR
            1091, 1092, 1093, 1097, 1098, 1099, 1103, 1104, 1105, 1109, 1110, 1111, 1115, 1116, 1117, 
            // GSM
            1121, 1122, 1123, 1127, 1128, 1129, 1133, 1134, 1135, 1139, 1140, 1141, 1145, 1146, 1147 
        };

        public static HashSet<uint> IshguardGathererLeves = new()
        { 
            // FSH
            1211, 1212, 1213, 1217, 1218, 1219, 1223, 1224, 1225, 1229, 1230, 1231, 1235, 1236, 1237
        };

        public static unsafe int Allowances => QuestManager.Instance()->NumLeveAllowances;
        public static unsafe TimeSpan NextAllowances => QuestManager.GetNextLeveAllowancesDateTime() - DateTime.Now;
        public static int GetNumAcceptedLeveQuests() => GetActiveLeveIds().Count();
        public static unsafe LeveWork* GetLeveWork(uint leveId)
        {
            var leveQuests = QuestManager.Instance()->LeveQuests;

            for (var i = 0; i < leveQuests.Length; i++)
            {
                if (leveQuests[i].LeveId == (ushort)leveId)
                {
                    return leveQuests.GetPointer(i);
                }
            }

            return null;
        }
        public static unsafe bool IsStarted(uint leveId)
        {
            var leveWork = GetLeveWork((ushort)leveId);
            if (leveWork == null)
                return false;

            return leveWork->Sequence == 1 && leveWork->ClearClass != 0;
        }
        public static unsafe bool IsComplete(uint leveID)
            => QuestManager.Instance()->IsLevequestComplete((ushort)leveID);
        public static bool IsAccepted(uint leveID)
            => GetActiveLeveIds().Any(id => id == (ushort)leveID);
        public static unsafe IEnumerable<ushort> GetActiveLeveIds()
        {
            var leveIds = new HashSet<ushort>();

            foreach (ref var entry in QuestManager.Instance()->LeveQuests)
            {
                if (entry.LeveId != 0)
                    leveIds.Add(entry.LeveId);
            }

            return leveIds;
        }
    }
}
