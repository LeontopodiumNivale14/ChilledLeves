using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Text;

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
                    
                    if (LeveInfo.LeveNpc_Info.TryGetValue(leve.Npc_Vendor, out var vendor))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        Theme_Colors.BodyText($"Start Location");

                        ImGui.TableNextColumn();
                        Theme_Colors.BodyText($"{ExcelHelper.Sheet_TerritoryType.GetRow(vendor.TerritoryId).PlaceName.Value.Name}");

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        Theme_Colors.BodyText($"Leve Vendor");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"{vendor.Name}"))
                        {
                            Utils.SetFlagForNPC(vendor.TerritoryId, vendor.Npc_Flag.X, vendor.Npc_Flag.Y);
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
