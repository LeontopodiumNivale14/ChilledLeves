using ChilledLeves.Gui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility;
using System.Globalization;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_SelectionTab
    {
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

                var globalScale = ImGuiHelpers.GlobalScale;
                Vector2 imageSize = new Vector2(18 * globalScale, 18 * globalScale);

                foreach (var leveId in sortedLeves)
                {
                    if (!LeveFilter(leveId.Key))
                        continue;

                    ImGui.TableNextRow();

                    bool isSelected = Leve_DetailsTab.selectedLeve == leveId.Key;

                    ImGui.TableSetColumnIndex(0);
                    ImGui.AlignTextToFramePadding();

                    // Selectable makes it to where 
                    ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0, 0, 0, 0));
                    ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 0));
                    ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0, 0, 0, 0));
                    bool clicked = ImGui.Selectable($"{leveId.Key}", isSelected, ImGuiSelectableFlags.SpanAllColumns);
                    ImGui.PopStyleColor(3);

                    bool isHovered = ImGui.IsItemHovered();

                    if (clicked)
                        Leve_DetailsTab.selectedLeve = leveId.Key;

                    if (ImGui.IsItemHovered() && (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) || (ImGui.IsItemClicked(ImGuiMouseButton.Left) && C.RapidImport)))
                    {
                        if (!C.LeveOrder.Contains(leveId.Key))
                            C.LeveOrder.Add(leveId.Key);

                        if (C.LeveList[leveId.Key] == 0)
                            C.LeveList[leveId.Key] = 1;

                        C.SaveDebounced();
                    }

                    // Manually paint the row background — this is what actually gives you the full-row highlight
                    if (isSelected)
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderActive));
                    }
                    else if (isHovered)
                    {
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(ImGuiCol.HeaderHovered));
                    }

                    // JobIcon
                    ImGui.TableNextColumn();
                    GameIcons.DrawInlineOrIcon(LeveInfo.Job_IconDict[leveId.Value.Job].IconId, FontAwesomeIcon.Book);

                    // Favorite icon
                    ImGui.TableNextColumn();
                    if (C.FavoriteLeves.Contains(leveId.Key))
                    {
                        GameIcons.DrawInlineOrIcon(61817, FontAwesomeIcon.Star, new Vector4(1f, 0.84f, 0f, 1f));
                    }

                    ImGui.TableNextColumn();
                    if (Utils.Leve_IsComplete(leveId.Key))
                    {
                        ImGui.AlignTextToFramePadding();
                        GameIcons.DrawInlineOrIcon(071045, FontAwesomeIcon.Check);
                    }

                    ImGui.TableNextColumn();
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{leveId.Value.LeveName}");
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
                showLeve &= C.Job_LeveFilter[leve.Job];

                if (C.Leve_Filter["Completed"])
                    showLeve &= Utils.Leve_IsComplete(leveId);
                else if (C.Leve_Filter["Incomplete"])
                    showLeve &= !Utils.Leve_IsComplete(leveId);

                // Name comparison
                if (!string.IsNullOrEmpty(Leve_Name))
                {
                    var compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                    showLeve &= compareInfo.IndexOf(leve.LeveName, Leve_Name, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
                }

                // Level comparison
                showLeve &= C.MinLevel <= leve.Level && leve.Level <= C.MaxLevel;

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
