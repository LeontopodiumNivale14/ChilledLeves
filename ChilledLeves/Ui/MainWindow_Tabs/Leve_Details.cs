using ChilledLeves.Utilities.LeveData;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_Details
    {
        public static uint selectedLeve = 0;

        public static void Draw()
        {
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
                Theme_Colors.HeaderText($"Rewards");
                if (ImGui.BeginTable("Rewards Table", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableSetupColumn("Reward");

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

                if (ImGui.BeginTable("Leve_Npc Info", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable))
                {
                    ImGui.TableSetupColumn("Info Kind");
                    ImGui.TableSetupColumn("Npc Name");
                    ImGui.TableSetupColumn("External Link");

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    Theme_Colors.BodyText($"Leve Vendor");
                    if (LeveInfo.LeveNpc_Info.TryGetValue(leve.Npc_Vendor, out var vendor))
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.Button($"{vendor.Name}"))
                        {
                            SetFlagForNPC(vendor.TerritoryId, vendor.Npc_Flag.X, vendor.Npc_Flag.Y);
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
                            SetFlagForNPC(turninNpc.TerritoryId, turninNpc.Npc_Flag.X, turninNpc.Npc_Flag.Y);
                        }
                    }

                    ImGui.EndTable();
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
