using System.Collections.Generic;
using System.Globalization;
using ChilledLeves.Utilities;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;

public static partial class SettingsUi
{
    private static readonly string MountOption = "Mount Options";

    private static SettingEntry UseMount = new()
    {
        Label = "Use Mount",
        Category = MountOption,
        Keywords = new[] { "Mount", "Fly" },
        Draw = () =>
        {
            var v = C.UseMount;
            if (ImGui.Checkbox("Use Mount", ref v))
            {
                C.UseMount = v;
                C.Save();
            }
        }
    };

    private static SettingEntry OptionalFly = new()
    {
        Label = "Optional Fly",
        Category = MountOption,
        Keywords = new[] { "Fly" },
        Draw = () =>
        {
            var v = C.OptionalFly;
            if (ImGui.Checkbox("Allow Flying", ref v))
            {
                C.OptionalFly = v;
                C.Save();
            }
        }
    };

    private static SettingEntry MountMinDistance = new()
    {
        Label = "Minimum Mount Distance",
        Category = MountOption,
        Keywords = new[] { "Mount", "Distance" },
        Draw = () =>
        {
            var v = C.Mount_MinDistance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("Minimum Mount Distance", ref v))
            {
                C.Mount_MinDistance = v;
                C.SaveDebounced();
            }
        }
    };

    private static SettingEntry MountDismountDistance = new()
    {
        Label = "Dismount Distance",
        Category = MountOption,
        Keywords = new[] { "Mount", "Dismount", "Distance" },
        Draw = () =>
        {
            var v = C.Mount_DismountDistance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("Dismount Distance", ref v))
            {
                C.Mount_DismountDistance = v;
                C.SaveDebounced();
            }
        }
    };

    private static SettingEntry FlyMinDistance = new()
    {
        Label = "Minimum Fly Distance",
        Category = MountOption,
        Keywords = new[] { "Fly", "Distance" },
        Draw = () =>
        {
            var v = C.Fly_MinDistance;
            ImGui.SetNextItemWidth(200);
            if (ImGui.InputInt("Minimum Fly Distance", ref v))
            {
                C.Fly_MinDistance = v;
                C.SaveDebounced();
            }
        }
    };

    private static SettingEntry MountSelection = new()
    {
        Label = "Select Mount",
        Category = MountOption,
        Keywords = new[] { "Mount", "Select", "Picker", "Choose" },
        Draw = DrawMountSelector
    };

    private static Dictionary<uint, string> _availableMounts = new();
    private static string _mountSearchText = "";
    private static int _mountDisplayOffset = 0;
    private static int _mountItemsPerPage = 10;

    private static void DrawMountSelector()
    {
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

    private static unsafe void OpenMountPopup()
    {
        _availableMounts.Clear();
        _availableMounts[0] = "Mount Roulette";
        var mountSheet = ExcelHelper.Sheet_Mount;
        foreach (var mountItem in mountSheet)
        {
            if (!PlayerState.Instance()->IsMountUnlocked(mountItem.RowId)) continue;

            string name = CultureInfo.CurrentCulture.TextInfo
                .ToTitleCase(mountItem.Singular.ToString().ToLower());
            _availableMounts[mountItem.RowId] = name;
        }
        _mountSearchText = "";
        _mountDisplayOffset = 0;
        ImGui.OpenPopup("Mount Options");
    }
}