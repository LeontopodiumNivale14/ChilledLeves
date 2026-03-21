using ChilledLeves.Utilities.LeveData;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_WorklistTab
    {
        private static readonly ImGuiEx.RealtimeDragDrop<uint> _leveDragDrop = new("LeveOrder", id => id.ToString());

        public static void Draw()
        {
            List<uint> levesToRemove = new();
            _leveDragDrop.Begin();

            if (ImGui.BeginTable("Leve Selection", 6, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##drag");
                ImGui.TableSetupColumn("Id");
                ImGui.TableSetupColumn("Job");
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Run Amount");
                ImGui.TableSetupColumn("Remove");

                ImGui.TableHeadersRow();

                for (var i = 0; i < C.LeveOrder.Count; i++)
                {
                    var leve = C.LeveOrder[i];
                    var sheetInfo = LeveInfo.Leve_SheetInfo[leve];
                    ImGui.PushID($"{leve}_{sheetInfo.LeveName}");

                    ImGui.TableNextRow();
                    _leveDragDrop.NextRow();
                    _leveDragDrop.SetRowColor(leve.ToString());

                    ImGui.TableSetColumnIndex(0);
                    _leveDragDrop.DrawButtonDummy(leve.ToString(), C.LeveOrder, i);

                    ImGui.TableNextColumn();
                    ImGui.Text($"{leve}");

                    ImGui.TableNextColumn();
                    ImGui.Image(LeveInfo.Job_IconDict[sheetInfo.Job].ColorIcon.GetWrapOrEmpty().Handle, new(24, 24));

                    ImGui.TableNextColumn();
                    ImGui.Text($"{sheetInfo.LeveName}");

                    ImGui.TableNextColumn();
                    var runAmount = C.LeveList[leve];
                    ImGui.SetNextItemWidth(200);
                    if (ImGui.SliderInt("##RunAmount", ref runAmount, 1, 100))
                    {
                        C.LeveList[leve] = runAmount;
                        C.SaveDebounced();
                    }

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                    {
                        levesToRemove.Add(leve);
                    }

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }

            foreach (var leve in levesToRemove)
            {
                C.LeveOrder.Remove(leve);
                C.Save();
            }

            _leveDragDrop.End();
        }
    }
}
