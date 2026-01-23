using Dalamud.Interface.Textures;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public class Leve_SheetData
    {
        public string LeveName { get; set; }
        public uint JobAssignmentType { get; set; }
        public uint Job { get; set; }
        public uint Level { get; set; }
        public uint Npc_Vendor { get; set; }
        public uint Npc_Turnin { get; set; }
        public uint QuestID { get; set; }
        public int ExpReward { get; set; }
        public int GilReward { get; set; }
        public int AllowanceCost { get; set; }
        public Material_Turnin MaterialInfo { get; set; }
    }

    public class Material_Turnin
    {
        public uint Item_Id { get; set; }
        public string Item_Name { get; set; }
        public ISharedImmediateTexture? Item_Icon { get; set; }
        public int TurninAmount { get; set; }
        public int RepeatAmount { get; set; }
    }

    public static Dictionary<uint, Leve_SheetData> Leve_SheetInfo = new();

    public static void PopulateLeveInfo()
    {
        var leve_Sheet = Svc.Data.GetExcelSheet<Leve>();

        if (leve_Sheet != null)
        {
            foreach (var row in leve_Sheet)
            {
                if (row.LeveClient.RowId == 0)
                    continue;

                var assignmentType = row.LeveAssignmentType.RowId;
                if (!Assigned_LeveJobs.Contains(assignmentType))
                    continue;

                var id = row.RowId;
                string leveName = row.Name.ToString();
                leveName = leveName.Replace("<nbsp>", " ");
                leveName = leveName.Replace("<->", "");

                var leveClient = row.LeveClient.RowId;

                var job = (uint)row.ClassJobCategory.RowId;
                var level = row.ClassJobLevel;
                var leve_Vendor = LeveNpc_Info.Where(x => x.Value.Leves.Contains(id)).FirstOrDefault().Key;
                var leve_Turnin = TurninNpcId(leveClient, assignmentType);
                var questID = row.DataId.RowId;
                var exp = row.ExpReward.ToInt();
                var gilReward = row.ExpReward.ToInt();
                var allowanceCost = row.AllowanceCost.ToInt();

                Material_Turnin materialList = new();
                if (Material_LeveJobs.Contains(assignmentType))
                {

                }

                if (!Leve_SheetInfo.ContainsKey(id))
                {
                    Leve_SheetInfo.Add(id, new()
                    {
                        LeveName = leveName,
                        JobAssignmentType = assignmentType,
                        Job = job,
                        Level = level,
                        Npc_Vendor = leve_Vendor,
                        Npc_Turnin = leve_Turnin,
                        QuestID = questID,
                        ExpReward = exp,
                        GilReward = gilReward,
                        AllowanceCost = allowanceCost,

                    });
                }
            }
        }
    }
}
