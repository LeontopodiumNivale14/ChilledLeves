using ECommons.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChilledLeves.Resources
{
    public class WorklistPresetLoader
    {
        public static Dictionary<string, SavedWorklist> Presets { get; private set; } = new();

        private static readonly string PresetDirectory = Path.Combine(Svc.PluginInterface.GetPluginConfigDirectory(), "worklists");

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        private static readonly JsonSerializerOptions WriteOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // ── Load ──────────────────────────────────────────────────────────────

        public static void LoadAllPresets()
        {
            Presets.Clear();
            Directory.CreateDirectory(PresetDirectory);

            foreach (var file in Directory.GetFiles(PresetDirectory, "*.json"))
            {
                try
                {
                    var preset = JsonSerializer.Deserialize<SavedWorklist>(
                        File.ReadAllText(file), ReadOptions);

                    if (preset == null || string.IsNullOrWhiteSpace(preset.Name)) continue;

                    // Use filename (without extension) as the stable key
                    var key = Path.GetFileNameWithoutExtension(file);
                    Presets[key] = preset;
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Failed to load preset {Path.GetFileName(file)}: {ex.Message}");
                }
            }

            PluginLog.Information($"Loaded {Presets.Count} worklist presets");
        }

        // ── Save ──────────────────────────────────────────────────────────────

        public static string SavePreset(SavedWorklist preset)
        {
            Directory.CreateDirectory(PresetDirectory);

            // Filenames created with the date+time to not collide with each other... lets users to atleast figure out which one they wanna do
            var fileName = $"{SanitizeName(preset.Name)}_{preset.CreatedAt:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(PresetDirectory, fileName);

            File.WriteAllText(filePath, JsonSerializer.Serialize(preset, WriteOptions));
            Presets[Path.GetFileNameWithoutExtension(fileName)] = preset;

            PluginLog.Verbose($"Saved preset '{preset.Name}' -> {fileName}");
            return filePath;
        }

        public static void DeletePreset(string key)
        {
            var filePath = Path.Combine(PresetDirectory, $"{key}.json");
            if (File.Exists(filePath))
                File.Delete(filePath);
            Presets.Remove(key);
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public static SavedWorklist? GetPreset(string key) => Presets.TryGetValue(key, out var p) ? p : null;

        public static List<SavedWorklist> GetPresetsSortedByDate() => Presets.Values.OrderByDescending(p => p.CreatedAt).ToList();

        // ── Internals ─────────────────────────────────────────────────────────

        private static string SanitizeName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public class SavedWorklist
        {
            public string Name { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public List<LeveEntry> Entries { get; set; } = new List<LeveEntry>();
        }

        public class LeveEntry
        {
            public uint LeveID { get; set; }
            public int InputValue { get; set; } = 0;
        }
    }
}