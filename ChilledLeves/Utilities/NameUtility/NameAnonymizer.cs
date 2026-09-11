using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace ChilledLeves.Utilities.NameUtility
{
    /// <summary>
    /// Used to get fake names for the purpose of screenshotting...
    /// That way users don't have to out themselves as plugin users, and keeps it anon
    /// </summary>
    internal static partial class NameAnonymizer
    {
        private static readonly Random Rng = new();

        // Used to cache the name so we don't redraw the name every single frame
        // As funny as that would be... (it would be pretty funny ngl)
        private static readonly ConcurrentDictionary<string, string> Cache = new();

        /// <summary>
        /// Gets (or creates) a stable fake "First Last" name for the given cache key.
        /// Pass something unique per character, e.g. $"{world}_{realName}".
        /// </summary>
        public static string GetFakeName(string key)
        {
            return Cache.GetOrAdd(key, _ => GenerateRandomName());
        }

        /// <summary>
        /// Forces a new random name for this key, overwriting any cached value.
        /// Use this for a manual "reroll" button.
        /// </summary>
        public static string Reroll(string key)
        {
            var name = GenerateRandomName();
            Cache[key] = name;
            return name;
        }

        /// <summary>
        /// Clears all cached fake names (e.g. if the user toggles anonymization off and on
        /// later and wants fresh names rather than picking up old ones).
        /// </summary>
        public static void ClearCache() => Cache.Clear();

        private static string GenerateRandomName()
        {
            var first = FirstNames[Rng.Next(FirstNames.Length)];
            var last = LastNames[Rng.Next(LastNames.Length)];
            return $"{first} {last}";
        }
    }
}
