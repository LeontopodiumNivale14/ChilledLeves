using ChilledLeves.Utilities;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Game_GuildLeves
    {
        public static void Draw()
        {
            if (GenericHelpers.TryGetAddonMaster<GuildLeve>("GuildLeve", out var guild) && guild.IsAddonReady)
            {
                if (ImGui.BeginTable("Leve Details: Debugger", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Level");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Select Leve");

                    foreach (var leve in guild.Levequests)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{leve.Level}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{leve.Name}");

                        ImGui.TableNextColumn();
                        if (ImGui.Button($"Select##Select_{leve.Name}"))
                        {
                            leve.Select();
                        }
                    }

                    ImGui.EndTable();
                }
            }

            if (GenericHelpers.TryGetAddonMaster<JournalDetail>("JournalDetail", out var journal) && journal.IsAddonReady)
            {

            }
        }
    }
}
