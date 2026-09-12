using ChilledLeves.Enums;
using ChilledLeves.Gui;
using ChilledLeves.Scheduler;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Leve_Playlist
    {
        private static readonly ImGuiEx.RealtimeDragDrop<uint> _leveDragDrop = new("LeveOrder", id => id.ToString());

        public static void Draw()
        {
            var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

            using (var child = ImRaii.Child("Worklist: Leve Playlist", new(-1, -1), true))
            {
                if (!child.Success)
                    return;

                List<uint> levesToRemove = new();

                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Play, "Start Playlist", Leve_Helper.State == LeveState.Idle))
                {
                    Leve_Helper.SelectedMode = ModeSelection.Standard;
                    Leve_Helper.State = LeveState.CheckLeves;
                }
                ImGui.SameLine();
                if (ImGuiEx.IconButtonWithText(FontAwesomeIcon.Square, "Stop", Leve_Helper.State != LeveState.Idle))
                {
                    SchedulerMain.DisablePlugin();
                }

                bool allowGrabMulti = C.GrabMulti;
                if (ImGui.Checkbox("Allow grabbing multiple leves", ref allowGrabMulti))
                {
                    C.GrabMulti = allowGrabMulti;
                    C.Save();
                }

                bool allowMultiTurnin = C.AllowMultiTurnin;
                if (ImGui.Checkbox("Allow Multi-Turnin Leves", ref allowMultiTurnin))
                {
                    C.AllowMultiTurnin = allowMultiTurnin;
                    C.Save();
                }
                ImGui_Ice.IconWithTooltip(FontAwesomeIcon.QuestionCircle,
                    "If a leve has multiple turnin, this will allow you to do said multiple turnins of the leve.\n" +
                    "This really only applies for specific leves below Lv. 80, as post that they stopped doing this.\n" +
                    "Enabling this will update the counts of leves that allow it");

                _leveDragDrop.Begin();

                if (C.LeveOrder.Count != 0)
                {
                    if (ImGui.BeginTable("Leve Selection", 8, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                    {
                        ImGui.TableSetupColumn("##drag");
                        ImGui.TableSetupColumn("Id");
                        ImGui.TableSetupColumn("Job");
                        ImGui.TableSetupColumn("Name");
                        ImGui.TableSetupColumn("Run Amount");
                        ImGui.TableSetupColumn("Need");
                        ImGui.TableSetupColumn("Have");
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
                            GameIcons.DrawInlineOrIcon(LeveInfo.Job_IconDict[sheetInfo.Job].IconId, FontAwesomeIcon.Book);

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

                            var currentAmount = 0;
                            var neededAmount = 0;

                            if (LeveInfo.LeveJobs_Material.Contains(sheetInfo.Job))
                            {
                                var materialInfo = sheetInfo.MaterialInfo;
                                var itemId = materialInfo.Item_Id;

                                currentAmount = Utils.GetItemCount(itemId);
                                neededAmount = Utils.Leve_RequiredAmount(materialInfo) * runAmount;
                            }

                            ImGui.TableNextColumn();
                            if (neededAmount != 0)
                                ImGui.Text($"{neededAmount}");

                            ImGui.TableNextColumn();
                            if (neededAmount != 0)
                                ImGui.Text($"{currentAmount}");

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
                }
                else
                {
                    ImGui.Text("We don't have any leves currently added, please add some from the Leve Selection Tab");
                }

                _leveDragDrop.End();
            }
        }
    }
}
