using ChilledLeves.Enums;
using ChilledLeves.Utilities.LeveData;
using ChilledLeves.Utilities.OldUtils;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
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

        public static LeveClass SelectedJob
        {
            get => sliderIndex == 0 ? LeveClass.All : (LeveClass)(sliderIndex + 7);
            set
            {
                if (value == LeveClass.All)
                    sliderIndex = 0;
                else
                    sliderIndex = (int)value - 7;
            }
        }

        private static string[] jobLabels = { "All Jobs", "CRP", "BSM", "ARM", "GSM",
                                              "LTW", "WVR", "ALC", "CUL", "MIN", "BTN", "FSH" };

        public static void Draw()
        {
            uint JobId(LeveClass classId)
            {
                uint jobid = classId switch
                {
                    LeveClass.Crp => 8,
                    LeveClass.Bsm => 9,
                    LeveClass.Arm => 10,
                    LeveClass.Gsm => 11,
                    LeveClass.Ltw => 12,
                    LeveClass.Wvr => 13,
                    LeveClass.Alc => 14,
                    LeveClass.Cul => 15,
                    LeveClass.Min => 16,
                    LeveClass.Btn => 17,
                    LeveClass.Fsh => 18,
                    _ => 0,
                };

                return jobid;
            }

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
                if (ImGui.BeginTable("Leve Sheet Info", 12, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Reorderable))
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
                    ImGui.TableSetupColumn("Gathering Location");

                    ImGui.TableHeadersRow();

                    foreach (var entry in LeveInfo.Leve_SheetInfo)
                    {
                        if (!Old_Utils.ContainsIgnoreSpacesAndCase(entry.Value.LeveName, missionName))
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
                        else if (LeveInfo.Gathering_LeveJobs.Contains(entry.Value.JobAssignmentType))
                        {
                            ImGuiEx.Icon(FontAwesomeIcon.ArrowUpRightFromSquare);
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.BeginTooltip();
                                string gatherPointIds = string.Join(", ", entry.Value.Gather_NodeInfo.NodeIds);
                                ImGui.Text($"{gatherPointIds}");
                                ImGui.EndTooltip();
                            }
                        }

                        ImGui.TableNextColumn();
                        if (ImGui.Button("Test Ring"))
                        {

                        }

                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
