using ChilledLeves.Enums;
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
        public ExpansionIds Expansion { get; set; } = ExpansionIds.ARR;
        public Job Job { get; set; } = Job.ADV;
        public uint Level { get; set; } = 0;
        public uint Npc_Vendor { get; set; } = 0;
        public uint Npc_Turnin { get; set; } = 0;
        public uint QuestID { get; set; } = 0;
        public int ExpReward { get; set; } = -1;
        public int GilReward { get; set; } = -1;
        public int AllowanceCost { get; set; } = -1;
        public LeveKind Stringtype { get; set; } = LeveKind.Battlecraft;
        public MapInfo Gather_MapInfo { get; set; } = new();
        public Material_Turnin MaterialInfo { get; set; } = new();
        public Gathering_Turnin Gather_NodeInfo { get; set; } = new();
    }

    public class Material_Turnin
    {
        public uint Item_Id { get; set; } = 0;
        public string Item_Name { get; set; } = "???";
        public ISharedImmediateTexture? Item_Icon { get; set; } = null;
        public int TurninAmount { get; set; } = -1;
        public int RepeatAmount { get; set; } = -1;
    }

    public class Gathering_Turnin
    {
        public List<uint> NodeIds { get; set; } = new();
        public List<GatherItems> GatherItems { get; set; } = new();
    }

    public class GatherItems
    {
        public uint ItemId { get; set; } = 0;
        public int Amount { get; set; } = 0;
    }
    public class MapInfo
    {
        public Vector3 Location { get; set; } = Vector3.Zero;
        public uint TerritoryId { get; set; } = 0;
        public int Radius { get; set; } = 0;
    }

    public static Dictionary<uint, Leve_SheetData> Leve_SheetInfo = new();

    public static Dictionary<LeveKind, string> Leve_SelectText = new();

    public static void UpdateSelectString()
    {
        List<(LeveKind type, uint value)> leveList = new()
        {
            new() {type = LeveKind.Battlecraft, value = 1},
            new() {type = LeveKind.Fieldcraft, value = 2},
            new() {type = LeveKind.Tradecraft, value = 3},
            new() {type = LeveKind.LS_Battlecraft, value = 14},
            new() {type = LeveKind.LS_Fieldcraft, value = 15},
            new() {type = LeveKind.LS_Tradecraft, value = 16},
            new() {type = LeveKind.TurninLeve, value = 13},
        };

        foreach (var kind in leveList)
        {
            Leve_SelectText[kind.type] = ExcelHelper.Sheet_leveText.GetRow(kind.value).Text1.ExtractText();
        }
    }

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

                var job = (Job)row.ClassJobCategory.RowId - 1;
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
                var gilReward = row.GilReward.ToInt();
                var allowanceCost = row.AllowanceCost.ToInt();

                ExpansionIds expansion = level switch
                {
                    < 50 => ExpansionIds.ARR,
                    < 60 => ExpansionIds.HW,
                    < 70 => ExpansionIds.StB,
                    < 80 => ExpansionIds.ShB,
                    < 90 => ExpansionIds.EW,
                    < 100 => ExpansionIds.DT,
                    _ => ExpansionIds.Unk  // fallback for 100+
                };

                var type = LeveKind.Battlecraft;
                if (allowanceCost == 1)
                {
                    type = (int)job switch
                    {
                        < 16 => LeveKind.Tradecraft,
                        < 19 => LeveKind.Fieldcraft,
                        _ => LeveKind.Battlecraft,
                    };
                }
                else if (allowanceCost == 10)
                {
                    type = (int)job switch
                    {
                        < 16 => LeveKind.LS_Tradecraft,
                        < 19 => LeveKind.LS_Fieldcraft,
                        _ => LeveKind.LS_Battlecraft,
                    };
                }

                Material_Turnin materialList = new();
                Gathering_Turnin gatheringList = new();
                MapInfo mapInfo = new();
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
                else if (Gathering_LeveJobs.Contains(assignmentType))
                {
                    List<uint> gatherPoints = new();
                    List<GatherItems> gatherItems = new();

                    var gatherLeveId = row.DataId.RowId;
                    var levelData = row.LevelStart.Value;
                    if (Svc.Data.GetExcelSheet<GatheringLeve>().TryGetRow(gatherLeveId, out var gatheringLeve))
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (gatheringLeve.Route[i].RowId != 0)
                            {
                                var route = gatheringLeve.Route[i].Value;
                                for (int x = 0; x < 12; x++)
                                {
                                    var gPointId = route.GatheringPoint[x].RowId;
                                    PluginLog.Debug($"Currently viewing nodeID: {gPointId}");
                                    if (!gatherPoints.Contains(gPointId) && gPointId != 0)
                                        gatherPoints.Add(gPointId);
                                }
                            }

                            if (gatheringLeve.RequiredItem[i].RowId != 0)
                            {
                                var itemId = gatheringLeve.RequiredItem[i].RowId;
                                var amount = gatheringLeve.RequiredItemQuantity[i];

                                GatherItems reqItems = new()
                                {
                                    ItemId = itemId,
                                    Amount = amount,
                                };
                                gatherItems.Add(reqItems);
                            }
                        }
                    }

                    mapInfo.Location = new(levelData.X, levelData.Y, levelData.Z);
                    mapInfo.TerritoryId = levelData.Territory.RowId;
                    mapInfo.Radius = (int)levelData.Radius;


                    gatheringList.GatherItems = gatherItems;
                    gatheringList.NodeIds = gatherPoints;
                }

                if (!Leve_SheetInfo.ContainsKey(id))
                {
                    Leve_SheetInfo.Add(id, new()
                    {
                        LeveName = leveName,
                        JobAssignmentType = assignmentType,
                        Job = job,
                        Expansion = expansion,
                        Level = level,
                        Npc_Vendor = leve_Vendor,
                        Npc_Turnin = leve_Turnin,
                        QuestID = questID,
                        ExpReward = exp,
                        GilReward = gilReward,
                        AllowanceCost = allowanceCost,
                        MaterialInfo = materialList,
                        Stringtype = type,
                        Gather_NodeInfo = gatheringList,
                        Gather_MapInfo = mapInfo,
                    });
                }
            }
        }

        UpdateJobIcons();
    }

    public static void UpdateLeves()
    {
        foreach (var leve in Leve_SheetInfo)
        {
            if (!C.LeveList.ContainsKey(leve.Key))
                C.LeveList[leve.Key] = 0;

            C.SaveDebounced();
        }
    }
}
