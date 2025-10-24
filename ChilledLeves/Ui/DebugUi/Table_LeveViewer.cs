using ChilledLeves.Utilities.LeveUtilities;
using ECommons.ExcelServices;
using InteropGenerator.Runtime.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.DebugUi
{
    internal class Table_LeveViewer
    {
        private static string _IDSearch = "";
        private static string _NameSearch = "";

        public static void Draw()
        {
            ImGuiTableFlags tableFlags = ImGuiTableFlags.RowBg |
                                         ImGuiTableFlags.Borders |
                                         ImGuiTableFlags.Reorderable |
                                         ImGuiTableFlags.Hideable |
                                         ImGuiTableFlags.Resizable | 
                                         ImGuiTableFlags.SizingFixedFit;

            if (ImGui.BeginTable("Leve Dictionary Viewer", 3, tableFlags))
            {
                ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 50);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthStretch);

                ImGui.TableHeadersRow();

                // Adding custom filter row
                ImGui.TableNextRow();

                // Column 0 | ID Search 
                ImGui.TableSetColumnIndex(0);
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##IDSearch", "ID", ref _IDSearch);

                // Column 1 | Name Search
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##NameSearch", "Leve Name", ref _NameSearch);

                // Column 2 | Job 
                ImGui.TableNextColumn();

                foreach (var leve in LeveInfo.LeveDictionary)
                {
                    if (!string.IsNullOrEmpty(_IDSearch) && !leve.Key.ToString().Contains(_IDSearch, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.IsNullOrEmpty(_NameSearch) && !leve.Value.LeveName.ToString().Contains(_NameSearch, StringComparison.OrdinalIgnoreCase))
                        continue;

                    ImGui.TableNextRow();

                    // ID
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text($"{leve.Key}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{leve.Value.LeveName}");

                    ImGui.TableNextColumn();
                    var jobId = leve.Value.JobId;
                    Svc.Texture.TryGetFromGameIcon(LeveInfo.JobInfo[jobId].ColorIconId, out var colorIcon);
                    ImGui.Image(colorIcon.GetWrapOrEmpty().Handle, new Vector2(25, 25));
                }
                ImGui.EndTable();
            }
        }
    }
}
