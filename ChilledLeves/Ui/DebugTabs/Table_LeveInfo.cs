using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_LeveInfo
    {
        private static string missionName = string.Empty;
        private static int MinLevel = 0;
        private static int MaxLevel = 100;

        private static int sliderIndex = 0;
        public static uint SelectedJob
        {
            get => sliderIndex == 0 ? 0u : (uint)(sliderIndex + 7);
            set
            {
                if (value == 0)
                    sliderIndex = 0;
                else if (value >= 8 && value <= 18)
                    sliderIndex = (int)value - 7;
            }
        }

        private static string[] jobLabels = { "All Jobs", "CRP", "BSM", "ARM", "GSM",
                                              "LTW", "WVR", "ALC", "CUL", "MIN", "BTN", "FSH" };

        public static void Draw()
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Mission Name", ref missionName);
            ImGui.SetNextItemWidth(200);
            ImGui.SliderInt("Min Level", ref MinLevel, 0, MaxLevel);
            ImGui.SetNextItemWidth(200);
            ImGui.SliderInt("Max Level", ref MaxLevel, MinLevel, 100);
            ImGui.SetNextItemWidth(200);
            ImGui.SliderInt("Filter##JobFilter", ref sliderIndex, 0, 11, jobLabels[sliderIndex]);

            using (var tableInfo = ImRaii.Child("Table Info", new Vector2(0, 0)))
            {
                if (ImGui.BeginTable("Leve Sheet Info", 11, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable))
                {
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Job");
                    ImGui.TableSetupColumn("Assignment");
                    ImGui.TableSetupColumn("Level");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Exp");
                    ImGui.TableSetupColumn("Gil");
                    ImGui.TableSetupColumn("Cost");
                    ImGui.TableSetupColumn("Vendor");
                    ImGui.TableSetupColumn("Turnin");
                    ImGui.TableSetupColumn("Material");

                    ImGui.TableHeadersRow();

                    foreach (var entry in LeveInfo.Leve_SheetInfo)
                    {
                        if (!string.IsNullOrEmpty(missionName) && !entry.Value.LeveName.Contains(missionName.ToLower()))
                            continue;

                        if (entry.Value.Level < MinLevel)
                            continue;

                        if (entry.Value.Level > MaxLevel)
                            continue;

                        if (SelectedJob != 0 && entry.Value.Job != SelectedJob)
                            continue;


                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{entry.Key}");

                        ImGui.TableNextColumn();
                        var jobId = entry.Value.Job;
                        ImGui.Image(LeveInfo.Job_IconDict[jobId].ColorIcon.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                        ImGui.SameLine(0, 0);
                        ImGui.Image(LeveInfo.GreyJobIcon(jobId).Handle, new Vector2(24, 24));

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.JobAssignmentType}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Level}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.LeveName}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.ExpReward}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.GilReward}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.AllowanceCost}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Npc_Vendor}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{entry.Value.Npc_Turnin}");

                        ImGui.TableNextColumn();
                        if (LeveInfo.Material_LeveJobs.Contains(entry.Value.JobAssignmentType))
                        {
                            ImGuiEx.Icon(FontAwesomeIcon.ArrowUpRightFromSquare);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                var materialInfo = entry.Value.MaterialInfo;

                                ImGui.Image(materialInfo.Item_Icon.GetWrapOrEmpty().Handle, new Vector2(24, 24));
                                ImGui.Text($"ID {materialInfo.Item_Id}");
                                ImGui.Text($"Name {materialInfo.Item_Name}");
                                ImGui.Text($"Item Amount {materialInfo.TurninAmount}");
                                ImGui.Text($"Turnin Amount {materialInfo.TurninAmount}");
                                ImGui.EndTooltip();
                            }
                        }

                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
