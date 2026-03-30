using ChilledLeves.Enums;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_LeveInfo
    {
        private static string missionName = string.Empty;
        private static int MinLevel = 0;
        private static int MaxLevel = 100;

        private static int sliderIndex = 0;

        public static Job SelectedJob
        {
            get => sliderIndex == 0 ? Job.ADV : (Job)(sliderIndex + 7);
            set
            {
                if (value == Job.ADV)
                    sliderIndex = 0;
                else
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
                    ImGui.TableSetupColumn("String Type");

                    ImGui.TableHeadersRow();

                    foreach (var entry in LeveInfo.Leve_SheetInfo)
                    {
                        if (!Utils.ContainsIgnoreSpacesAndCase(entry.Value.LeveName, missionName))
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
                        if (LeveInfo.LeveJobs_Material.Contains(entry.Value.Job))
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
                        else if (LeveInfo.LeveJobs_Gathering.Contains(entry.Value.Job))
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
                        ImGui.Text($"{entry.Value.LeveType}");

                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
