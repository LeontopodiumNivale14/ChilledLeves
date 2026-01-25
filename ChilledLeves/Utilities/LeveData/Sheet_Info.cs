using Dalamud.Interface.Textures;
using ECommons.ExcelServices;
using ECommons.Logging;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities.LeveData;

public static partial class LeveInfo
{
    public class Leve_SheetData
    {
        public string LeveName { get; set; } = "???";
        public uint JobAssignmentType { get; set; } = 0;
        public uint Job { get; set; } = 0;
        public uint Level { get; set; } = 0;
        public uint Npc_Vendor { get; set; } = 0;
        public uint Npc_Turnin { get; set; } = 0;
        public uint QuestID { get; set; } = 0;
        public int ExpReward { get; set; } = -1;
        public int GilReward { get; set; } = -1;
        public int AllowanceCost { get; set; } = -1;
        public Material_Turnin MaterialInfo { get; set; } = new();
    }

    public class Material_Turnin
    {
        public uint Item_Id { get; set; } = 0;
        public string Item_Name { get; set; } = "???";
        public ISharedImmediateTexture? Item_Icon { get; set; } = null;
        public int TurninAmount { get; set; } = -1;
        public int RepeatAmount { get; set; } = -1;
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

                PluginLog.Debug($"Leve: {id} being checked");

                var leveClient = row.LeveClient.RowId;

                var job = (uint)row.ClassJobCategory.RowId - 1;
                var level = row.ClassJobLevel;
                var leveVendorEntry = LeveNpc_Info.Where(x => x.Value.Leves.Contains(id)).FirstOrDefault();
                if (leveVendorEntry.Key == 0) // or check if leveVendorEntry.Value == null
                {
                    PluginLog.Warning($"Leve {id} ({leveName}) has no vendor in LeveNpc_Info, skipping");
                    continue;
                }
                var leve_Vendor = leveVendorEntry.Key;
                var leve_Turnin = TurninNpcId(leveClient, assignmentType);
                var questID = row.DataId.RowId;
                var exp = row.ExpReward.ToInt();
                var gilReward = row.ExpReward.ToInt();
                var allowanceCost = row.AllowanceCost.ToInt();

                Material_Turnin materialList = new();
                if (Material_LeveJobs.Contains(assignmentType))
                {
                    if (Svc.Data.GetExcelSheet<CraftLeve>().TryGetRow(questID, out var materialInfo))
                    {
                        var itemInfo = materialInfo.Item[0];
                        
                        var itemId = itemInfo.RowId;
                        string itemName = itemInfo.Value.Name.ToString();
                        var iconId = (uint)Svc.Data.GetExcelSheet<Item>().GetRow(itemId).Icon;
                        Svc.Texture.TryGetFromGameIcon(iconId, out var iconImage);
                        var repeatAmount = materialInfo.Repeats.ToInt() + 1;
                        int turninAmount = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            turninAmount += materialInfo.ItemCount[i].ToInt();
                        }

                        materialList.Item_Id = itemId;
                        materialList.Item_Name = itemName;
                        materialList.Item_Icon = iconImage;
                        materialList.RepeatAmount = repeatAmount;
                        materialList.TurninAmount = turninAmount;
                    }
                    else
                    {
                        PluginLog.Verbose($"No crafting info for: {id}");
                    }
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
                        MaterialInfo = materialList,
                    });
                }
            }
        }

        UpdateJobIcons();
    }
}
