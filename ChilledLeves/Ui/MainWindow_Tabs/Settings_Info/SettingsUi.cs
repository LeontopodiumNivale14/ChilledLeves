using Dalamud.Interface.Utility.Raii;
using System.Collections.Generic;

namespace ChilledLeves.Ui.MainWindow_Tabs.Settings_Info;

public static partial class SettingsUi
{
    public sealed class SettingEntry
    {
        public required string Label;
        public required string Category;
        public string[] Keywords = Array.Empty<string>();
        public required Action Draw;

        // Precomputed lowercase haystack so filtering isn't re-concatenating strings every frame.
        public string SearchHaystack => _haystack ??= string.Join(' ', new[] { Label, Category }.Concat(Keywords)).ToLowerInvariant();
        private string? _haystack;
    }

    // One entry per SettingEntry field across all partials. Add a line here whenever
    // a new partial file defines a new entry — this is the single place that ties them together.
    public static List<SettingEntry> BuildRegistry() => new()
    {
        UseMount,
        OptionalFly,
        MountMinDistance,
        MountDismountDistance,
        FlyMinDistance,
        MountSelection,
        FanSelection,

        // Sound Options
        EnableSound,
        SelectedSound,
        GetChatNotification,
        ShowAlertWindow,
        LeveAlertAmount,

        // Misc
        ColorTheme,
        ShowActiveOverlay,
    };

    // Built once and cached — BuildRegistry() constructs fresh SettingEntry wrappers,
    // so we don't want to call it every frame.
    private static List<SettingEntry>? _allSettings;
    private static List<SettingEntry> AllSettings => _allSettings ??= BuildRegistry();

    private static string _searchQuery = string.Empty;
    private static List<SettingEntry> _filtered = new();
    private static bool _filterDirty = true;

    public static void Draw()
    {
        var childColors = C.UseIceTheme ? ImRaii.PushColor(ImGuiCol.ChildBg, Theme_Colors.ChildBg) : default;

        using (ImRaii.Child("Settings Ui: Window", ImGui.GetContentRegionAvail(), true))
        {
            DrawSearchBar();
            ImGui.Separator();
            ImGui.Spacing();

            if (string.IsNullOrWhiteSpace(_searchQuery))
            {
                DrawGrouped(AllSettings);
            }
            else
            {
                if (_filterDirty)
                    Refilter();

                if (_filtered.Count == 0)
                    ImGui.TextDisabled($"No settings match \"{_searchQuery}\".");
                else
                    DrawGrouped(_filtered);
            }
        }
    }

    private static void DrawSearchBar()
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputTextWithHint("##settings-search", "Search settings...", ref _searchQuery, 128))
            _filterDirty = true;

        if (!string.IsNullOrEmpty(_searchQuery))
        {
            ImGui.SameLine();
            if (ImGui.SmallButton("Clear"))
            {
                _searchQuery = string.Empty;
                _filterDirty = true;
            }
        }
    }

    private static void Refilter()
    {
        var needle = _searchQuery.ToLowerInvariant();
        _filtered = AllSettings
            .Where(s => s.SearchHaystack.Contains(needle, StringComparison.Ordinal))
            .ToList();
        _filterDirty = false;
    }

    private static void DrawGrouped(List<SettingEntry> settings)
    {
        foreach (var group in settings.GroupBy(s => s.Category))
        {
            if (ImGui.CollapsingHeader(group.Key, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent();
                foreach (var entry in group)
                    entry.Draw();
                ImGui.Unindent();
                ImGui.Spacing();
            }
        }
    }
}