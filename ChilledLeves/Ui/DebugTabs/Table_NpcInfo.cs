using ChilledLeves.Scheduler.Tasks;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using Dalamud.Interface.Utility.Raii;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_NpcInfo
    {
        private static string _nameSearch = "";
        private static uint selectedNpc = 0;

        public static void Draw()
        {
            var navTask = P.navTask;
            var pluginTask = P.taskManager;

            if (ImGui.Button("Stop Plugin Task"))
            {
                if (navTask.IsBusy)
                {
                    navTask.Tasks.Clear();
                    navTask.AbortCurrent();
                }
                if (pluginTask.IsBusy)
                {
                    pluginTask.Tasks.Clear();
                    pluginTask.AbortCurrent();
                }
            }

            if (navTask.IsBusy)
            {
                ImGui.SameLine();
                string taskName = navTask.CurrentTask != null ? navTask.CurrentTask.Name : "???";

                ImGui.Text($"Nav Task: {taskName}");
            }

            ImGui.SetNextItemWidth(200);
            ImGui.InputText("Name Search", ref _nameSearch);

            using (var npcChild = ImRaii.Child("Debug: NPC Info Table", ImGui.GetContentRegionAvail()))
            {
                if (!npcChild.Success)
                    return;

                if (ImGui.BeginTable("ChilledLeves: Npc Info", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Location");

                    foreach (var entry in LeveInfo.LeveNpc_Info.OrderBy(x => x.Value.TerritoryId))
                    {
                        if (!Utils.ContainsIgnoreSpacesAndCase(entry.Value.Name, _nameSearch))
                            continue;

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        if (selectedNpc == entry.Key)
                        {
                            ImGui.TableSetBgColor(ImGuiTableBgTarget.RowBg0, ImGui.GetColorU32(new Vector4(0.0f, 1.0f, 0.2f, 0.25f)));
                        }

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{entry.Value.Name}");
                        ImGui.SameLine();
                        if (ImGui.SmallButton($"{entry.Key}"))
                        {
                            selectedNpc = entry.Key;
                            ImGui.SetClipboardText($"{entry.Key}");
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{TerritoryName.GetTerritoryName(entry.Value.TerritoryId)}");

                        ImGui.SameLine();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, $"Flag: {entry.Key}"))
                        {
                            Utils.SetFlagForNPC(entry.Value.TerritoryId, entry.Value.Npc_Flag.X, entry.Value.Npc_Flag.Y);
                        }

                        ImGui.TableNextColumn();
                        if (ImGuiEx.IconButton(FontAwesomeIcon.Plane, $"TP {entry.Key}"))
                        {
                            selectedNpc = entry.Key;
                            var task = P.navTask;
                            if (!task.IsBusy)
                            {
                                Task_Travel.AethernetTask_Grab(entry.Value);
                            }
                        }
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.Text($"Territory: {entry.Value.TerritoryId}");
                            ImGui.Text($"Position: {entry.Value.Npc_InteractZone}");
                            ImGui.EndTooltip();
                        }
                        ImGui.SameLine();

                        if (ImGuiEx.IconButton(FontAwesomeIcon.Running, $"Travel {entry.Key}"))
                        {
                            selectedNpc = entry.Key;
                            P.navmesh.PathfindAndMoveTo(entry.Value.Npc_InteractZone, false);
                        }
                    }

                    ImGui.EndTable();
                }
            }
        }
    }
}
