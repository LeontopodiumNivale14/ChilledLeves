using ChilledLeves.Utilities;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Ui.MainWindow_Tabs
{
    internal class Settings_Tab
    {
        public static void Draw()
        {
            bool useMount = C.UseMount;
            bool optionalFly = C.OptionalFly;

            int mount_MinDistance = C.Mount_MinDistance;
            int mount_DismountDistance = C.Mount_DismountDistance;
            int fly_MinDistance = C.Fly_MinDistance;

            uint mountId = C.MountId;
            string mountName = C.MountName;
            

            if (ImGui.Checkbox("Allow Flying", ref optionalFly))
            {
                C.OptionalFly = optionalFly;
                C.Save();
            }
            if (ImGui.Checkbox("Use Mount", ref useMount))
            {
                C.UseMount = useMount;
                C.Save();
            }
            if (ImGui.InputInt("Minimum Mount Distance", ref mount_MinDistance))
            {
                C.Mount_MinDistance = mount_MinDistance;
                C.SaveDebounced();
            }
            if (ImGui.InputInt("Dismount Distance", ref mount_DismountDistance))
            {
                C.Mount_DismountDistance = mount_DismountDistance;
                C.SaveDebounced();
            }
            if (ImGui.InputInt("Minimum Fly Distance", ref fly_MinDistance))
            {
                C.Fly_MinDistance = fly_MinDistance;
                C.SaveDebounced();
            }

            if (ImGui.Button("Select Mounting Option"))
                OpenMountPopup();
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"Mount: {C.MountName}");

            if (ImGui.BeginPopup("Mount Options"))
            {
                ImGui.InputText("Search", ref _mountSearchText, 100);

                var filtered = _availableMounts
                    .Where(kvp => string.IsNullOrEmpty(_mountSearchText) ||
                                  kvp.Value.Contains(_mountSearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                int total = filtered.Count;
                int maxOffset = Math.Max(0, total - _mountItemsPerPage);
                _mountDisplayOffset = Math.Min(_mountDisplayOffset, maxOffset);

                foreach (var mount in filtered.Skip(_mountDisplayOffset).Take(_mountItemsPerPage))
                {
                    if (ImGui.Selectable($"{mount.Value}##{mount.Key}"))
                    {
                        C.MountId = mount.Key; 
                        C.MountName = mount.Value;
                        C.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }

                ImGui.Separator();
                if (ImGui.Button("Previous") && _mountDisplayOffset > 0)
                    _mountDisplayOffset = Math.Max(0, _mountDisplayOffset - _mountItemsPerPage);
                ImGui.SameLine();
                ImGui.Text($"{_mountDisplayOffset + 1}-{Math.Min(_mountDisplayOffset + _mountItemsPerPage, total)} of {total}");
                ImGui.SameLine();
                if (ImGui.Button("Next") && _mountDisplayOffset < maxOffset)
                    _mountDisplayOffset = Math.Min(maxOffset, _mountDisplayOffset + _mountItemsPerPage);

                ImGui.EndPopup();
            }
        }

        private static Dictionary<uint, string> _availableMounts = new();
        private static string _mountSearchText = "";
        private static int _mountDisplayOffset = 0;
        private static int _mountItemsPerPage = 10;

        private static unsafe void OpenMountPopup()
        {
            _availableMounts.Clear();
            _availableMounts[0] = "Mount Roulette";
            var mountSheet = ExcelHelper.Sheet_Mount;
            foreach (var mountItem in mountSheet)
            {
                if (!PlayerState.Instance()->IsMountUnlocked(mountItem.RowId)) continue;

                string name = System.Globalization.CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(mountItem.Singular.ToString().ToLower());
                _availableMounts[mountItem.RowId] = name;
            }
            _mountSearchText = "";
            _mountDisplayOffset = 0;
            ImGui.OpenPopup("Mount Options");
        }
    }
}
