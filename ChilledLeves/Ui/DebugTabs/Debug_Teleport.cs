using ChilledLeves.Utilities;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Debug_Teleport
    {
        public static int TestTeleport = 0;

        public static unsafe void Draw()
        {
            var agent = AgentTelepotTown.Instance();
            if (agent == null || agent->Data == null)
            {
                ImGui.Text("Aethernet List is not available currently");
                return;
            }

            var data = agent->Data;

            using (var table = ImRaii.Table("##Debug_AethernetTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                if (!table)
                    return;

                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Status");
                ImGui.TableSetupColumn("##Teleport");
                ImGui.TableHeadersRow();

                for (byte i = 0; i < data->AetheryteCount; i++)
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    var entry = data->Entries[i];

                    var name = ExcelHelper.Sheet_Aetheryte!.TryGetRow(entry.AetheryteId, out var row)
                        ? row.AethernetName.Value.Name.ExtractText()
                        : $"#{entry.AetheryteId}";
                    ImGui.Text(name);

                    ImGui.TableNextColumn();
                    ImGui.Text($"ID: {entry.AetheryteId} Byte: {i}");

                    ImGui.TableNextColumn();
                    DrawFlagIcon(entry);

                    ImGui.TableNextColumn();
                    using (ImRaii.PushId($"Aetheryte_{i}"))
                    {
                        var targetId = Svc.Targets.Target.BaseId;
                        bool currentlyAt = targetId == entry.AetheryteId;

                        using (ImRaii.Disabled(currentlyAt))
                        {
                            if (ImGui.SmallButton("Go"))
                                agent->TeleportToAetheryte(i);
                        }
                    }
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text("Test");

                ImGui.TableNextColumn();
                ImGui.InputInt("TeleportID", ref TestTeleport);

                ImGui.TableSetColumnIndex(3);
                if (ImGui.Button("Test go"))
                {
                    agent->TeleportToAetheryte((byte)TestTeleport);
                }
            }
        }
        private static void DrawFlagIcon(AgentTelepotTownData.AetheryteEntry entry)
        {
            var targetId = Svc.Targets.Target.BaseId;
            bool currentlyAt = targetId == entry.AetheryteId;

            var anyTrue = entry.IsLocked || entry.IsUnusable || entry.IsAetheryte || entry.IsCurrent || currentlyAt;

            using (ImRaii.PushFont(UiBuilder.IconFont))
                ImGui.TextUnformatted(anyTrue ? FontAwesomeIcon.Flag.ToIconString() : FontAwesomeIcon.Circle.ToIconString());


            if (ImGui.IsItemHovered())
            {
                using var tooltip = ImRaii.Tooltip();
                if (tooltip.Alive)
                {
                    DrawFlagLine("LastVisited", entry.IsCurrent);
                    DrawFlagLine("IsLocked", entry.IsLocked);
                    DrawFlagLine("Oneway", entry.IsUnusable);
                    DrawFlagLine("IsAetheryte", entry.IsAetheryte);
                    DrawFlagLine("IsCurrent", currentlyAt);
                }
            }
        }

        private static void DrawFlagLine(string label, bool value)
        {
            using (ImRaii.PushColor(ImGuiCol.Text, value
                       ? new Vector4(0.4f, 1f, 0.4f, 1f)
                       : new Vector4(0.5f, 0.5f, 0.5f, 1f)))
            {
                ImGui.TextUnformatted($"{label}: {value}");
            }
        }
    }
}
