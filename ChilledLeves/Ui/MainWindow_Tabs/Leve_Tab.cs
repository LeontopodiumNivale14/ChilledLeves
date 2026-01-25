using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_Tab
    {
        public static int Leve_MinLevel = 1;
        public static int Leve_MaxLevel = 100;
        public static string Leve_Name = "";
        public static int LeveCount = 0;
        public static int Leve_Total = 0;

        public static void Draw()
        {
            var sortedLeves = LeveInfo.Leve_SheetInfo
                                .OrderBy(x => x.Value.Job)
                                .ThenBy(x => x.Key)
                                .ToList();
            LeveCount = 0;
            Leve_Total = sortedLeves.Count();

            if (ImGui.BeginTable("Leve List Info", 5, ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("ID");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Favorite");
                ImGui.TableSetupColumn("Complete");
                ImGui.TableSetupColumn("Name");

                foreach (var leveId in sortedLeves)
                {
                    if (LeveFilter(leveId.Key))
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (ImGui.Selectable($"##{leveId.Key}", Leve_Details.selectedLeve == leveId.Key, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            Leve_Details.selectedLeve = leveId.Key;
                        }
                        if (ImGui.IsItemHovered() && (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) || (ImGui.IsItemClicked(ImGuiMouseButton.Left) && C.RapidImport)))
                        {
                            if (C.LeveList[leveId.Key] == 0)
                                C.LeveList[leveId.Key] = 1;
                        }
                        ImGui.SameLine();
                        Theme_Colors.BodyText($"{leveId.Key}");

                        // JobIcon
                        ImGui.TableNextColumn();
                        var globalScale = ImGuiHelpers.GlobalScale;
                        Vector2 imageSize = new Vector2(18 * globalScale, 18 * globalScale);
                        ImGui.Image(LeveInfo.Job_IconDict[leveId.Value.Job].ColorIcon.GetWrapOrEmpty().Handle, imageSize);

                        // Favorite icon
                        ImGui.TableNextColumn();
                        if (C.FavoriteLeves.Contains(leveId.Key))
                        {
                            var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
                            Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
                            Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);
                            ImGui.Image(starTex.Handle, imageSize, uvMin, uvMax);
                            ImGui.SameLine(0, 4);
                        }

                        ImGui.TableNextColumn();
                        if (IsComplete(leveId.Key))
                        {
                            var CompleteTexture = LeveStatusDict[1].GetWrapOrEmpty();
                            ImGui.Image(CompleteTexture.Handle, imageSize);
                            ImGui.SameLine(0, 4);
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{leveId.Value.LeveName}");
                    }
                }

                ImGui.EndTable();
            }
        }

        private static bool LeveFilter(uint leveId)
        {
            if (LeveInfo.Leve_SheetInfo.TryGetValue(leveId, out var leve))
            {
                bool showLeve = true;

                if (C.Leve_Filter["Favorites"])
                {
                    showLeve &= C.FavoriteLeves.Contains(leveId);
                }

                // Make sure that it's enabled
                var jobName = Main_Tab.GetJobName(leve.Job);
                showLeve &= C.Job_Filter[jobName];

                if (C.Leve_Filter["Completed"])
                    showLeve &= IsComplete(leveId);
                else if (C.Leve_Filter["Incomplete"])
                    showLeve &= !IsComplete(leveId);

                // Name comparison
                if (!string.IsNullOrEmpty(Leve_Name))
                {
                    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                    showLeve &= compareInfo.IndexOf(leve.LeveName, Leve_Name, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
                }

                // Level comparison
                showLeve &= Leve_MinLevel <= leve.Level && leve.Level <= Leve_MaxLevel;

                if (showLeve)
                    LeveCount += 1;

                return showLeve;
            }
            else
            {
                return false;
            }
        }
    }
}
