using ChilledLeves.Scheduler.Tasks;
using ChilledLeves.Utilities;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Table_Aethernet
    {
        public static unsafe void Draw()
        {
            var currentTerritory = Player.Territory.RowId;

            var aetherShards = Utils.Aethernet.Where(x => x.Value.ValidTerritories.Contains(currentTerritory)).ToList();
            if (_PathCalculations != null && _PathCalculations.IsCompleted)
            {
                _PathCalculations = null;
            }
            if (aetherShards.Count != 0)
            {
                using (var table = ImRaii.Table("Aethernet Info", 7, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("ID");
                    ImGui.TableSetupColumn("Position");
                    ImGui.TableSetupColumn("Distance to");
                    ImGui.TableSetupColumn("Move");
                    ImGui.TableSetupColumn("Teleport");
                    ImGui.TableHeadersRow();

                    foreach (var entry in aetherShards)
                    {
                        using (ImRaii.PushId($"Aethernet: {entry.Value.ShardId}"))
                        {
                            bool sameTerritory = entry.Value.TerritoryId == Player.Territory.RowId;

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            var name = ExcelHelper.Sheet_Aetheryte!.TryGetRow(entry.Key, out var row)
                                ? row.AethernetName.Value.Name.ExtractText()
                                : $"#{entry.Key}";
                            ImGui.Text(name);

                            ImGui.TableNextColumn();
                            ImGui.Text($"{entry.Key}");

                            ImGui.TableNextColumn();
                            ImGui.Text($"{entry.Value.Position.X:N2}, {entry.Value.Position.Y:N2}, {entry.Value.Position.Z:N2}");

                            ImGui.TableNextColumn();
                            using (ImRaii.Disabled(!sameTerritory))
                            {
                                if (ImGui.Button($"Calculate##DistanceTo_{entry.Key}"))
                                {
                                    UpdateDistance(entry);
                                }
                                ImGui.SameLine();
                                ImGui.Text($"{entry.Value.DistanceTo:N2}");
                            }

                            ImGui.TableNextColumn();
                            using (ImRaii.Disabled(!sameTerritory))
                            {
                                if (ImGui.Button("Move to"))
                                {
                                    P.navmesh.PathfindAndMoveTo(entry.Value.MoveTo, false);
                                }
                            }

                            ImGui.TableNextColumn();
                            var agent = AgentTelepotTown.Instance();

                            uint targetId = 0;
                            if (Svc.Targets.Target != null)
                            {
                                targetId = Svc.Targets.Target.BaseId;
                            }
                            bool currentlyAt = targetId == entry.Key;
                            bool allow = !currentlyAt && agent != null && agent->Data != null;

                            using (ImRaii.Disabled(!allow))
                            {
                                if (ImGui.Button("Aethernet Go"))
                                {
                                    var data = agent->Data;
                                    for (byte i = 0; i < data->AetheryteCount; i++)
                                    {
                                        if (data->Entries[i].AetheryteId == entry.Key)
                                        {
                                            agent->TeleportToAetheryte(i);
                                            return;
                                        }
                                    }
                                }
                            }
                            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                            {
                                ImGui.BeginTooltip();
                                ImGui.Text($"Currently At: {currentlyAt}");
                                ImGui.Text($"Same Territory: {sameTerritory}");
                                ImGui.Text($"Agent on? {agent != null}");
                                ImGui.EndTooltip();
                            }
                        }
                    }

                }
            }
            else
            {
                ImGui.Text("There are no valid aethershards here.");
            }
        }

        private static void UpdateDistance(KeyValuePair<uint, Utils.AethershardInfo> entry)
        {
            var playerPos = Player.Position;
            entry.Value.PathList = new();
            if (_PathCalculations == null)
            {
                _PathCalculations = Task.Run(async () =>
                {
                    entry.Value.PathList = await FindPath(playerPos, entry.Value.MoveTo);
                });
            }
        }
        private static Task? _PathCalculations = null;
        public static async Task<List<Vector3>> FindPath(Vector3 position, Vector3 destination)
        {
            return await P.navmesh.Pathfind(position, destination, false);
        }
    }
}
