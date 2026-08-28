using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

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
                Theme_Colors.BodyText($"[{leve.Level}] {leve.LeveName}");
                ImGui.SameLine();
                ImGui.TextDisabled($"ID: {selectedLeve}");

                ImGui.Separator();
                Theme_Colors.HeaderText($"Leve Info/Rewards");
                if (ImGui.BeginTable("Rewards Table", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableSetupColumn("Reward");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    Theme_Colors.BodyText($"Level");
                    ImGui.TableNextColumn();
                    Theme_Colors.BodyText($"{leve.Level}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    Theme_Colors.BodyText("Experience");
                    ImGui.TableNextColumn();
                    Theme_Colors.BodyText($"{leve.ExpReward:N0}");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    Theme_Colors.BodyText("Gil");
                    ImGui.TableNextColumn();
                    Theme_Colors.BodyText($"{leve.GilReward:N0} ± 5%");

                    ImGui.EndTable();
                }

                ImGui.Separator();
                Theme_Colors.HeaderText("Npc Info");
                if (ImGui.BeginTable("Leve_Npc Info", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Info Kind");
                    ImGui.TableSetupColumn("Npc Name");

                    foreach (var vendor in leve.Npc_Vendors)
                    {
                        if (LeveInfo.LeveNpc_Info.TryGetValue(vendor, out var vendorInfo))
                        {
                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            Theme_Colors.BodyText($"Start Location");

                            ImGui.TableNextColumn();
                            Theme_Colors.BodyText($"{ExcelHelper.Sheet_TerritoryType.GetRow(vendorInfo.TerritoryId).PlaceName.Value.Name}");

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            Theme_Colors.BodyText($"Leve Vendor");

                            ImGui.TableNextColumn();
                            if (ImGui.Button($"{vendorInfo.Name}"))
                            {
                                Utils.SetFlagForNPC(vendorInfo.TerritoryId, vendorInfo.Npc_Flag.X, vendorInfo.Npc_Flag.Y);
                            }
                        }
                    }
                   

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    Theme_Colors.BodyText($"Leve Turnin");
                    if (LeveInfo.LeveNpc_Info.TryGetValue(leve.Npc_Turnin, out var turninNpc))
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.Button($"{turninNpc.Name}"))
                        {
                            Utils.SetFlagForNPC(turninNpc.TerritoryId, turninNpc.Npc_Flag.X, turninNpc.Npc_Flag.Y);
                        }
                    }

                    ImGui.EndTable();
                }

                if (LeveInfo.LeveJobs_Material.Contains(leve.Job))
                {
                    var materialInfo = leve.MaterialInfo;
                    var turninAmount = materialInfo.TurninAmount;
                    var repeatAmount = materialInfo.RepeatAmount;

                    if (repeatAmount > 1)
                    {
                        ImGui.Checkbox("Show for multiple turnins", ref ShowMultiTurnin);
                        if (ShowMultiTurnin)
                            turninAmount *= repeatAmount;
                    }

                    Vector2 imageSize = new(30 * globalScale, 30 * globalScale);
                    ImGui.Image(materialInfo.Item_Icon.GetWrapOrEmpty().Handle, imageSize);
                    ImGui.SameLine();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{materialInfo.Item_Name} | Required: {turninAmount}");
                }
                else if (LeveInfo.LeveJobs_Gathering.Contains(leve.Job))
                {
                    string kind = leve.GatheringRule switch
                    {
                        GatheringRule.Search => "Search",
                        GatheringRule.Procurance => "Procure",
                        GatheringRule.Search_Procurance => "Search & Procure",
                        GatheringRule.Execution => "Execution",
                        _ => $"{leve.GatheringRule}"
                    };
                    ImGui.Text($"Mission Kind: {kind}");
                    ImGui.SameLine();

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
                    ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle, ruleInfo);

                    if (leve.Gather_NodeInfo.GatherItems.Count > 0)
                    {
                        foreach (var item in leve.Gather_NodeInfo.GatherItems)
                        {
                            var itemId = item.ItemId;
                            var amount = item.Amount;

                            if (ExcelHelper.Sheet_EventItem.TryGetRow(itemId, out var eventItem))
                            {
                                if (eventItem.Icon is { } iconId && Svc.Texture.TryGetFromGameIcon((int)iconId, out var icon))
                                {
                                    ImGui_Ice.ImageButtonWithText(icon.GetWrapOrEmpty(), $"{amount}", $"{itemId}_leveItem", new(24, 24));
                                }
                            }
                        }
                    }

                }

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
    }
}
