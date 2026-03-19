using ECommons.Logging;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ChilledLeves.Resources
{
    public class RouteLoader
    {
        public static Dictionary<uint, GatheringRoute> Leve_Routes { get; set; } = new();

        private static readonly JsonSerializerOptions _readOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new Vector3Converter() }
        };

        private static readonly JsonSerializerOptions _writeOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new Vector3Converter() }
        };

        private static readonly Dictionary<uint, string> ExpansionNames = new()
        {
            { 0, "2.x - A Realm Reborn" },
            { 1, "3.x - Heavensward" },
            { 2, "4.x - Stormblood" },
            { 3, "5.x - Shadowbringers" },
            { 4, "6.x - Endwalker" },
            { 5, "7.x - Dawntrail" }
        };

        // ── Load ──────────────────────────────────────────────────────────────

        public static void LoadAllRoutes()
        {
            Leve_Routes.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames()
                .Where(n => n.Contains("Gather_Routes") && n.EndsWith(".json"));

            foreach (var resourceName in resources)
            {
                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream == null) continue;

                    using var reader = new StreamReader(stream);
                    var route = JsonSerializer.Deserialize<GatheringRoute>(reader.ReadToEnd(), _readOptions);

                    if (route == null || !ValidateRoute(route, resourceName)) continue;

                    if (Leve_Routes.ContainsKey(route.LeveId))
                    {
                        PluginLog.Warning($"Duplicate LeveId {route.LeveId} in {resourceName}, skipping");
                        continue;
                    }

                    Leve_Routes[route.LeveId] = route;
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Failed to load {resourceName}: {ex.Message}");
                }
            }

            PluginLog.Information($"Loaded {Leve_Routes.Count} leve gathering routes");
        }

        // ── Save ──────────────────────────────────────────────────────────────

        public static void SaveRoute(GatheringRoute route, string outputDirectory)
        {
            if (!ExpansionNames.TryGetValue(route.ExpansionId, out var expansionFolder))
            {
                PluginLog.Error($"Route {route.LeveId} has unknown ExpansionId {route.ExpansionId}");
                return;
            }

            var dir = Path.Combine(outputDirectory, expansionFolder, SanitizeName(route.ZoneName));
            Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, $"{route.LeveId}.json");
            File.WriteAllText(filePath, JsonSerializer.Serialize(route, _writeOptions));

            Leve_Routes[route.LeveId] = route;
            PluginLog.Verbose($"Saved leve route {route.LeveId} -> {Path.GetRelativePath(outputDirectory, filePath)}");
        }

        private static string SanitizeName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public static void SaveAllRoutes(string outputDirectory)
        {
            foreach (var route in Leve_Routes.Values)
                SaveRoute(route, outputDirectory);
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public static GatheringRoute? GetRoute(uint leveId)
            => Leve_Routes.TryGetValue(leveId, out var route) ? route : null;

        public static List<GatheringRoute> GetRoutesForTerritory(uint territoryId)
            => Leve_Routes.Values.Where(r => r.TerritoryId == territoryId).ToList();

        public static List<GatheringRoute> GetRoutesForJob(uint jobId)
            => Leve_Routes.Values.Where(r => r.GatheringJob == jobId).ToList();

        public static List<GatheringRoute> GetRoutesForExpansion(uint expansionId)
            => Leve_Routes.Values.Where(r => r.ExpansionId == expansionId).ToList();

        public static string GetExpansionDisplayName(uint expansionId)
            => ExpansionNames.TryGetValue(expansionId, out var name) ? name : $"Unknown ({expansionId})";

        // ── Internals ─────────────────────────────────────────────────────────

        private static bool ValidateRoute(GatheringRoute route, string source)
        {
            if (route.NodeInfo is null or { Count: 0 })
            {
                PluginLog.Warning($"{source}: no nodes");
                return false;
            }

            foreach (var node in route.NodeInfo)
            {
                if (node.BaseId == 0)
                {
                    PluginLog.Warning($"{source}: node has BaseId 0, skipping route");
                    return false;
                }
            }

            return true;
        }
    }
}