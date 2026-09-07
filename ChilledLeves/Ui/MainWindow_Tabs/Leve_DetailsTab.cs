using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Resources;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using SharpDX.Direct3D11;
using static ChilledLeves.Utilities.LeveData.LeveInfo;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_DetailsTab
    {
        public static uint selectedLeve = 0;
        public static bool ShowMultiTurnin = true;

        public static void Draw()
        {
            var globalScale = ImGuiHelpers.GlobalScale;

            if (LeveInfo.Leve_SheetInfo.TryGetValue(selectedLeve, out var leve))
            {
                var jobImage = LeveInfo.Job_IconDict[leve.Job].ColorIcon;
                ImGui.Image(jobImage.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText($"{leve.LeveName}");
                ImGui.SameLine();
                ImGui.TextDisabled($"ID: {selectedLeve}");

                LeveInfo_Table(leve);

                NpcDetails_Table(leve);

                CraftingDetails_Table(leve);
                GatheringDetails_Table(leve);

                float textLineHeight = ImGui.GetTextLineHeight();
                Vector2 buttonSize = new Vector2(ImGui.GetContentRegionAvail().X, textLineHeight * 1.5f);
                string worklist = C.LeveOrder.Contains(selectedLeve) ? "Remove Leve from Manifest" : "Add Leve to Manifest";

                if (ImGui.Button(worklist, buttonSize))
                {
                    if (C.LeveOrder.Contains(selectedLeve))
                        C.LeveOrder.Remove(selectedLeve);
                    else
                    {
                        C.LeveOrder.Add(selectedLeve);

                        if (C.LeveList[selectedLeve] == 0)
                            C.LeveList[selectedLeve] = 1;
                    }

                    C.Save();
                }

                string favorite = C.FavoriteLeves.Contains(selectedLeve) ? "Remove Leve from Favorites" : "Add Leve to Favorites";

                if (ImGui.Button(favorite, buttonSize))
                {
                    if (C.FavoriteLeves.Contains(selectedLeve))
                        C.FavoriteLeves.Remove(selectedLeve);
                    else
                        C.FavoriteLeves.Add(selectedLeve);
                }
            }
            else
            {
                // If none is selected
                float centerY = ImGui.GetWindowHeight() * 0.4f;
                ImGui.SetCursorPosY(centerY);
                float textWidth = ImGui.CalcTextSize("No Leve Selected").X;
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() - textWidth) * 0.5f);
                if (C.UseIceTheme)
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.85f, 1.0f, 0.7f), "No Leve Selected");
                }
                else
                {
                    ImGui.TextDisabled("No Leve Selected");
                }
                ImGui.Spacing();
                ImGui.Spacing();
                string hintText = "Select a leve from the list to view details";
                float hintWidth = ImGui.CalcTextSize(hintText).X;
                ImGui.SetCursorPosX((ImGui.GetWindowWidth() - hintWidth) * 0.5f);
                ImGui.TextDisabled(hintText);
            }
        }

        private static void LeveInfo_Table(LeveInfo.Leve_SheetData leve)
        {
            ImGui.Separator();
            Theme_Colors.HeaderText($"Leve Info/Rewards");

            using (var leveInfo = ImRaii.Table("Rewards Table", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                if (!leveInfo.Success)
                    return;

                ImGui.TableSetupColumn("Type");
                ImGui.TableSetupColumn("Reward");

                // Level Info
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText("Lv.");

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText($"{leve.Level}");

                // Experience 
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText("EXP");
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText($"{leve.ExpReward:N0}");

                // Gil Reward
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                GameIcons.DrawInlineOrIcon(65002, FontAwesomeIcon.Coins);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Gil");

                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText($"{leve.GilReward:N0} ± 5%");

                // Completion Status
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                GameIcons.DrawInlineOrIcon(LeveInfo.LeveStatus[Leve_Status.NotGrabbed], FontAwesomeIcon.CheckSquare);
                ImGui.SameLine();
                ImGui.AlignTextToFramePadding();
                Theme_Colors.BodyText($"Completed");

                ImGui.TableNextColumn();
                uint statusId = Utils.Leve_IsComplete(selectedLeve) ? LeveInfo.LeveStatus[Leve_Status.NotComplete] : LeveInfo.LeveStatus[Leve_Status.Complete];
                GameIcons.DrawInline(statusId, false);

                // TODO: Throw in potentional item rewards
            }
        }

        private static void NpcDetails_Table(LeveInfo.Leve_SheetData leve)
        {
            ImGui.Separator();
            Theme_Colors.HeaderText("Leve Vendors");

            using (var npcTable = ImRaii.Table("Leve_Npc Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                if (!npcTable.Success)
                    return;

                ImGui.TableSetupColumn("Location");
                ImGui.TableSetupColumn("Npc");

                ImGui.TableHeadersRow();
;
                foreach (var vendor in leve.Npc_Vendors)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    if (LeveInfo.LeveNpc_Info.TryGetValue(vendor, out var vendorInfo))
                    {
                        ImGui.AlignTextToFramePadding();
                        Theme_Colors.BodyText($"{ExcelHelper.Sheet_TerritoryType.GetRow(vendorInfo.TerritoryId).PlaceName.Value.Name}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"{vendorInfo.Name}"))
                        {
                            Utils.SetFlagForNPC(vendorInfo.TerritoryId, vendorInfo.Npc_Flag.X, vendorInfo.Npc_Flag.Y);
                        }
                    }
                    else
                    {
                        ImGui.AlignTextToFramePadding();
                        Theme_Colors.BodyText($"NpcId: ");

                        ImGui.TableNextColumn();
                        Theme_Colors.BodyText($"{vendor}");
                    }
                }
            }

            Theme_Colors.HeaderText("Turnin NPC");
            using (var turninNpc_Table = ImRaii.Table("Turnin_Npc Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
            {
                if (!turninNpc_Table.Success)
                    return;

                ImGui.TableSetupColumn("Location");
                ImGui.TableSetupColumn("Npc");

                ImGui.TableHeadersRow();

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (LeveInfo.LeveNpc_Info.TryGetValue(leve.Npc_Turnin, out var turninNpc))
                {
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText($"{ExcelHelper.Sheet_TerritoryType.GetRow(turninNpc.TerritoryId).PlaceName.Value.Name}");

                    ImGui.TableNextColumn();
                    if (ImGui.Button($"{turninNpc.Name}"))
                    {
                        Utils.SetFlagForNPC(turninNpc.TerritoryId, turninNpc.Npc_Flag.X, turninNpc.Npc_Flag.Y);
                    }
                }
            }
        }

        private static void CraftingDetails_Table(LeveInfo.Leve_SheetData leve)
        {
            if (LeveInfo.LeveJobs_Material.Contains(leve.Job))
            {
                ImGui.Separator();
                Theme_Colors.HeaderText("Crafting Details");

                var materialInfo = leve.MaterialInfo;
                var turninAmount = materialInfo.TurninAmount;
                var repeatAmount = materialInfo.RepeatAmount;

                if (repeatAmount > 1)
                {
                    ImGui.Checkbox("Show for multiple turnins", ref ShowMultiTurnin);
                    if (ShowMultiTurnin)
                        turninAmount *= repeatAmount;
                }

                using (var craftTableInfo = ImRaii.Table("Craft: LeveInfo", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
                {
                    if (!craftTableInfo.Success)
                        return;

                    ImGui.TableSetupColumn("##Icon");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Required");

                    ImGui.TableHeadersRow();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    GameIcons.DrawInline(materialInfo.IconId);

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText($"{materialInfo.Item_Name}");

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText($"{turninAmount:N0}");
                }
            }
        }

        private static void GatheringDetails_Table(LeveInfo.Leve_SheetData leve)
        {
            if (LeveInfo.LeveJobs_Gathering.Contains(leve.Job))
            {
                ImGui.Separator();
                Theme_Colors.HeaderText("Gathering Info");

                string kind = leve.GatheringRule switch
                {
                    GatheringRule.Search => "Search",
                    GatheringRule.Procurance => "Procure",
                    GatheringRule.Search_Procurance => "Search & Procure",
                    GatheringRule.Execution => "Execution",
                    _ => $"{leve.GatheringRule}"
                };
                string ruleInfo = leve.GatheringRule switch
                {
                    GatheringRule.Search => "Search 8 gathering nodes and gather them.\n" +
                        "All nodes must be searched",
                    GatheringRule.Procurance => "Gather at the 4 node locations",
                    GatheringRule.Search_Procurance => "Search 8 gathering nodes, and gather the required items\n" +
                        "All nodes must be searched, and the required items must be gathered",
                    GatheringRule.Execution => "Gather at the 4 node locations\n" +
                        "Bonus is gained by getting multiple gathering attempts?",
                    _ => "????"
                };

                using (var gatherTable = ImRaii.Table("Gathering: Info Table", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    if (!gatherTable.Success)
                        return;

                    ImGui.TableSetupColumn("Detail");
                    ImGui.TableSetupColumn("Info");

                    if (RouteLoader.Leve_Routes.TryGetValue(selectedLeve, out var routeInfo))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);

                        bool flyingRequired = routeInfo.FlyingNeeded;

                        if (!Utils.CanFly(routeInfo.TerritoryId) && flyingRequired)
                        {
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(EColor.Red));
                        }

                        GameIcons.DrawInline(60033, true);
                        ImGui.AlignTextToFramePadding();
                        Theme_Colors.BodyText($"Flying Required");

                        ImGui.TableNextColumn();
                        Theme_Colors.BodyText($"{flyingRequired}");
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText("Area");

                    ImGui.TableNextColumn();
                    var mapInfo = leve.Gather_MapInfo;
                    if (ExcelHelper.Sheet_TerritoryType.TryGetRow(mapInfo.TerritoryId, out var territoryName))
                    {
                        if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Flag, $"{territoryName.PlaceName.Value.Name}"))
                        {
                            Utils.SetGatheringRingFromWorld(mapInfo.TerritoryId, mapInfo.Location, mapInfo.Radius, $"Leve: {leve.LeveName}");
                        }
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText("Kind");

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    Theme_Colors.BodyText(kind);
                    ImGui.SameLine();
                    ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, ruleInfo);

                    if (leve.Gather_NodeInfo.GatherItems.Count > 0)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.AlignTextToFramePadding();
                        Theme_Colors.BodyText("Mission Goal");

                        ImGui.TableNextColumn();
                        foreach (var item in leve.Gather_NodeInfo.GatherItems)
                        {
                            var itemId = item.ItemId;
                            var amount = item.Amount;

                            if (ExcelHelper.Sheet_EventItem.TryGetRow(itemId, out var eventItem))
                            {
                                if (eventItem.Icon is { } iconId)
                                {
                                    ImGui_Ice.ImageButtonWithText(iconId, $"{amount}", $"{itemId}_leveItem");
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
