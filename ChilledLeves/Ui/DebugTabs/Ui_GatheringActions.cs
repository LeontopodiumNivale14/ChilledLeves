using ChilledLeves.Gui;
using ChilledLeves.Utilities.GatheringHelper;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;

namespace ChilledLeves.Ui.DebugTabs
{
    internal class Ui_GatheringActions
    {
        private static bool ViewOldButton = false;

        public static void Draw()
        {
            ImGui.Checkbox("View Old Buttons", ref ViewOldButton);

            using (var child = ImRaii.Child("Gathering Action Viewer"))
            {
                if (!child.Success)
                    return;

                using (var table = ImRaii.Table("Gathering Action Table", 6, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    if (!table.Success)
                        return;

                    ImGui.TableSetupColumn("Enum");
                    ImGui.TableSetupColumn("Name [Int]");
                    ImGui.TableSetupColumn("MIN");
                    ImGui.TableSetupColumn("BTN");
                    ImGui.TableSetupColumn("Level");
                    ImGui.TableSetupColumn("GP");

                    ImGui.TableHeadersRow();

                    foreach (var entry in Gather_Util.gathActionDict)
                    {
                        var key = entry.Key;
                        var value = entry.Value;

                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text($"{key}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{value.internalName}");

                        ImGui.TableNextColumn();
                        if (value.ClassAction.TryGetValue(Job.MIN, out var minerInfo))
                        {
                            if (ViewOldButton)
                            {
                                var icon = minerInfo.Icon.GetWrapOrEmpty();
                                ImGui_Ice.ImageButtonWithText(icon, $"[{minerInfo.ActionId}] - {minerInfo.Name}", $"MIN_{key}_{minerInfo.ActionId}", new Vector2(24, 24));
                            }
                            else
                            {
                                ImGui_Ice.ImageButtonWithText(minerInfo.IconId, $"[{minerInfo.ActionId}] - {minerInfo.Name}", $"MIN_{key}_{minerInfo.ActionId}");
                            }
                        }

                        ImGui.TableNextColumn();
                        if (value.ClassAction.TryGetValue(Job.BTN, out var botanistInfo))
                        {
                            if (ViewOldButton)
                            {
                                var icon = botanistInfo.Icon.GetWrapOrEmpty();
                                ImGui_Ice.ImageButtonWithText(icon, $"[{botanistInfo.ActionId}] - {botanistInfo.Name}", $"MIN_{key}_{botanistInfo.ActionId}", new Vector2(24, 24));
                            }
                            else
                            {
                                ImGui_Ice.ImageButtonWithText(botanistInfo.IconId, $"[{botanistInfo.ActionId}] - {botanistInfo.Name}", $"MIN_{key}_{botanistInfo.ActionId}");
                            }
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text($"{value.RequiredLv}");

                        ImGui.TableNextColumn();
                        ImGui.Text($"{value.RequiredGp}");
                    }
                }
            }
        }
    }
}
