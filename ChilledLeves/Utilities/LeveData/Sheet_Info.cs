using ChilledLeves.Enums;
using ChilledLeves.Utilities.LogInfo;
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
    public class Info_LeveSheetData
    {
        public string LeveName { get; set; } = "???";
        public AssignmentType JobAssignmentType { get; set; } = AssignmentType.None;
        public ExpansionIds Expansion { get; set; } = ExpansionIds.ARR;
        public Job Job { get; set; } = Job.ADV;
        public uint Level { get; set; } = 0;
        public List<uint> Npc_Vendors { get; set; } = new();
        public uint Npc_Turnin { get; set; } = 0;
        public uint QuestID { get; set; } = 0;
        public int ExpReward { get; set; } = -1;
        public int GilReward { get; set; } = -1;
        public int AllowanceCost { get; set; } = -1;
        public LeveKind LeveType { get; set; } = LeveKind.Battlecraft;
        public GatheringRule GatheringRule { get; set; } = GatheringRule.None;
        public Info_Map Gather_MapInfo { get; set; } = new();
        public Info_MaterialTurnin MaterialInfo { get; set; } = new();
        public Info_GatheringTurnin Gather_NodeInfo { get; set; } = new();
    }

    public class Info_MaterialTurnin
    {
        public uint Item_Id { get; set; } = 0;
        public string Item_Name { get; set; } = "???";
        public ISharedImmediateTexture? Item_Icon { get; set; } = null;
        public uint IconId { get; set; }
        public int TurninAmount { get; set; } = -1;
        public int RepeatAmount { get; set; } = -1;
    }

    public class Info_GatheringTurnin
    {
        public List<uint> NodeIds { get; set; } = new();
        public List<Info_GatherItems> GatherItems { get; set; } = new();
    }

    public class Info_GatherItems
    {
        public uint ItemId { get; set; } = 0;
        public int Amount { get; set; } = 0;
    }
    public class Info_Map
    {
        public Vector3 Location { get; set; } = Vector3.Zero;
        public uint TerritoryId { get; set; } = 0;
        public int Radius { get; set; } = 0;
    }

    public static Dictionary<uint, Info_LeveSheetData> Leve_SheetInfo = new();

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

    public static List<uint> GatherGrind_Npcs = new();

    /// <summary>
    /// Leves that are just garbage data... these shouldn't exist but do? But they also don't have any actual leve info. Might be relics of a time...
    /// </summary>
    private static readonly List<uint> IgnoreLeves = new() { 508, 514, 525, 531, 552, 554, 562, 564, 582, 597, 822, 827, 832 };

    public static void PopulateLeveInfo()
    {
        var leve_Sheet = Svc.Data.GetExcelSheet<Leve>();

        if (leve_Sheet != null)
        {
            foreach (var row in leve_Sheet)
            {
                if (row.LeveClient.RowId == 0)
                    continue;

                if (IgnoreLeves.Contains(row.RowId))
                    continue;

                var assignmentType = (AssignmentType)row.LeveAssignmentType.RowId;

                var id = row.RowId;
                string leveName = row.Name.ToString();
                leveName = leveName.Replace("<nbsp>", " ");
                leveName = leveName.Replace("<->", "");

                PluginLog.Debug($"Leve: {id} being checked");

                Job job = Job.GLA;
                if (!Assignment_Battle.Contains(assignmentType))
                {
                    job = (Job)row.ClassJobCategory.RowId - 1;
                }
                var level = row.ClassJobLevel;

                var potentionalClients = Levemete_Info.Where(x => x.Value.Leves.Contains(id));
                List<uint> leveVendors = new();
                foreach (var client in potentionalClients)
                {
                    leveVendors.Add(client.Key);
                }

                uint leve_Turnin = 0;

                var levelRow = row.LevelLevemete.ValueNullable;
                if (levelRow == null)
                {
                    IceLogging.Verbose($"Level info for this leve wasn't valid: {id}", "Sheet Building");
                }
                else
                {
                    leve_Turnin = levelRow.Value.Object.RowId;
                }
                var questID = row.DataId.RowId;
                var exp = row.ExpReward.ToInt();
                if (exp == 0 && job is Job.MIN or Job.BTN)
                {
                    var gatheringExp = ExcelHelper.Sheet_GatheringExp.GetRow((uint)level).Exp;
                    var multiplier = row.ExpFactor;

                    exp = (int)(multiplier * gatheringExp);
                }

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

                Info_MaterialTurnin materialList = new();
                Info_GatheringTurnin gatheringList = new();
                Info_Map mapInfo = new();
                GatheringRule rule = GatheringRule.None;

                if (Assignment_Crafters.Contains(assignmentType))
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
                        materialList.IconId = iconId;
                        materialList.RepeatAmount = repeatAmount;
                        materialList.TurninAmount = turninAmount;
                    }
                    else
                    {
                        PluginLog.Verbose($"No crafting info for: {id}");
                    }
                }
                else if (Assignment_Gathering.Contains(assignmentType))
                {
                    List<uint> gatherPoints = new();
                    List<Info_GatherItems> gatherItems = new();

                    var gatherLeveId = row.DataId.RowId;
                    var levelData = row.LevelStart.Value;
                    if (Svc.Data.GetExcelSheet<GatheringLeve>().TryGetRow(gatherLeveId, out var gatheringLeve))
                    {
                        rule = gatheringLeve.Rule.RowId switch
                        {
                            1 => GatheringRule.Search,
                            2 => GatheringRule.Procurance,
                            3 => GatheringRule.Search_Procurance,
                            4 => GatheringRule.Execution,
                            _ => GatheringRule.None,
                        };

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

                                Info_GatherItems reqItems = new()
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
                else if (Assignment_Battle.Contains(assignmentType))
                {
                    // Nothing atm, just pass it off like normal... hopefully...
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
                        Npc_Vendors = leveVendors,
                        Npc_Turnin = leve_Turnin,
                        QuestID = questID,
                        ExpReward = exp,
                        GilReward = gilReward,
                        AllowanceCost = allowanceCost,
                        MaterialInfo = materialList,
                        LeveType = type,
                        Gather_NodeInfo = gatheringList,
                        Gather_MapInfo = mapInfo,

                        GatheringRule = rule,
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

    public static void Update_ARRGrind()
    {
        var gatherGrindNpcs = Levemete_Info
            .Where(x => x.Value.Leves.Count > 0)
            .Where(x => x.Value.Leves.Any(y => Leve_SheetInfo[y].Job is Job.MIN or Job.BTN or Job.FSH))
            // .Where(x => x.Value.Leves.Any(y => Leve_SheetInfo[y].Level <= 60))
            .ToList();

        foreach (var npc in gatherGrindNpcs)
        {
            GatherGrind_Npcs.Add(npc.Key);
        }

        var arrList = C.ARR_LevemetPriority;

        foreach (var (id, list) in arrList)
        {
            if (list == null) continue;
            if (!LeveInfo.Levemete_Info.TryGetValue(id, out var npcInfo)) continue;

            var levesToRemove = list.Where(leve => !npcInfo.Leves.Contains(leve)).ToList();
            if (levesToRemove.Count == 0) continue;

            foreach (var leve in levesToRemove)
                list.Remove(leve);

            C.SaveDebounced();
        }

        foreach (var (npcId, npcInfo) in LeveInfo.Levemete_Info)
        {
            if (npcInfo.Leves.Count == 0) continue;

            if (!arrList.TryGetValue(npcId, out var priorityList))
            {
                priorityList = new List<uint>();
                arrList[npcId] = priorityList;
            }

            var changed = false;
            foreach (var leve in npcInfo.Leves)
            {
                if (!LeveInfo.Leve_SheetInfo.TryGetValue(leve, out var sheetInfo)) continue;
                if (sheetInfo.JobAssignmentType is not (AssignmentType.Miner or AssignmentType.Botanist or AssignmentType.Fisher)) continue;
                if (priorityList.Contains(leve)) continue;

                priorityList.Add(leve);
                changed = true;
            }

            if (changed)
                C.SaveDebounced();
        }
    }
}
