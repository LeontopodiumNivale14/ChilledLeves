using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.Throttlers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ChilledLeves.Utilities;

internal static class IceLogging
{
    private static string GetCallerPrefix()
    {
        var stackFrame = new StackFrame(3);
        var method = stackFrame.GetMethod();
        var className = method?.DeclaringType?.Name;
        var methodName = method?.Name;

        if (className != null && methodName != null)
        {
            return $"[{className}.{methodName}]";
        }
        else if (className != null)
        {
            return $"[{className}]";
        }
        else if (methodName != null)
        {
            return $"[{methodName}]";
        }
        return string.Empty;
    }

    private static string FormatMessage(string message, string prefix = null)
    {
        var callerPrefix = prefix ?? GetCallerPrefix();
        return $"{callerPrefix} {message}";
    }

    public static void Verbose(string message, string prefix = null, bool debugOnly = false)
    {
        var formattedMessage = FormatMessage(message, prefix);
        PluginLog.Verbose(formattedMessage);
        LogSystem.Log(LogLevel.Verbose, message, prefix);
    }

    public static void Debug(string message, string prefix = null, bool debugOnly = false)
    {
        LogSystem.Log(LogLevel.Debug, message, prefix);
        if (debugOnly)
        {
#if DEBUG
            var formattedMessage = FormatMessage(message, prefix);
            PluginLog.Debug(formattedMessage);
#endif
        }
        else
        {
            var formattedMessage = FormatMessage(message, prefix);
            PluginLog.Debug(formattedMessage);
        }
    }

    public static void Info(string message, string prefix = null, bool debugOnly = false)
    {
        LogSystem.Log(LogLevel.Info, message, prefix);
        if (debugOnly)
        {
#if DEBUG
            var formattedMessage = FormatMessage(message, prefix);
            PluginLog.Information(formattedMessage);
#endif
        }
        else
        {
            var formattedMessage = FormatMessage(message, prefix);
            PluginLog.Information(formattedMessage);
        }
    }

    public static void ChatInfo(string s, string prefix = null)
    {
        LogSystem.Log(LogLevel.Info, s, prefix);
        if (prefix == null)
        {
            if (EzThrottler.Throttle($"Throttling chat message: {s}", 1000))
            {
                Svc.Chat.Print(s);
                PluginLog.Information(s);
            }
        }
        else
        {
            if (EzThrottler.Throttle($"Throttling chat message: {s}", 1000))
            {
                Svc.Chat.Print($"{prefix} {s}");
                PluginLog.Information($"{prefix} {s}");
            }
        }
    }

    public static void ChatError(string s, string prefix = null)
    {
        LogSystem.Log(LogLevel.Error, s, prefix);
        if (prefix == null)
        {
            if (EzThrottler.Throttle($"Throttling chat message: {s}", 60000))
            {
                ECommons.ChatMethods.ChatPrinter.Red($"{s}");
                PluginLog.Error(s);
            }
        }
        else
        {
            if (EzThrottler.Throttle($"Throttling chat message: {s}", 60000))
            {
                ECommons.ChatMethods.ChatPrinter.Red($"{prefix} {s}");
                PluginLog.Error($"{prefix} {s}");
            }
        }
    }

    public static void Warning(string message, string prefix = null)
    {
        LogSystem.Log(LogLevel.Warning, message, prefix);
        var formattedMessage = FormatMessage(message, prefix);
        PluginLog.Warning(formattedMessage);
    }

    public static void Error(string message, string prefix = null)
    {
        LogSystem.Log(LogLevel.Error, message, prefix);
        var formattedMessage = FormatMessage(message, prefix);
        PluginLog.Error(formattedMessage);
    }

    public static void Fatal(string message, string prefix = null)
    {
        LogSystem.Log(LogLevel.Verbose, message, prefix);
        var formattedMessage = FormatMessage(message, prefix);
        PluginLog.Fatal(formattedMessage);
    }

    public enum LogLevel
    {
        Verbose,
        Debug,
        Info,
        Warning,
        Error
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public DateTime LastOccurrence { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public string? Category { get; set; }
        public int Count { get; set; } = 1;

        public LogEntry(LogLevel level, string message, string? category = null)
        {
            Timestamp = DateTime.Now;
            LastOccurrence = DateTime.Now;
            Level = level;
            Message = message;
            Category = category;
        }

        public bool IsRepeating => Count > 1;

        // Helper to create a unique key
        public string GetKey() => $"{Level}|{Category ?? ""}|{Message}";
    }

    public class DestinationEntry
    {
        public DateTime Timestamp { get; set; }
        public Vector3 PlayerStart { get; set; }
        public Vector3 PlayerDestination { get; set; }
        public float Distance { get; set; }

        public DestinationEntry(Vector3 end)
        {
            Timestamp = DateTime.Now;
            PlayerStart = Player.Position;
            PlayerDestination = end;
            Distance = Vector3.Distance(Player.Position, end);
        }
    }

    public static class DestinationLogs
    {
        private static List<DestinationEntry> logs = new();
        private static int maxDestinationCount = 3000;

        public static IReadOnlyList<DestinationEntry> Logs => logs.AsReadOnly();
        public static void Log(Vector3 end)
        {
            logs.Add(new DestinationEntry(end));
            if (logs.Count > maxDestinationCount)
                logs.RemoveAt(0);
        }
    }

    public static class LogSystem
    {
        private static List<LogEntry> logs = new();
        private static Dictionary<string, LogEntry> recentLogs = new(); // Track recent logs for time-window matching
        private static int maxLogCount = 1000;
        private static TimeSpan consolidationWindow = TimeSpan.FromMilliseconds(250); // Merge duplicates within 500ms

        public static IReadOnlyList<LogEntry> Logs => logs.AsReadOnly();

        public static void Log(LogLevel level, string message, string? category = null)
        {
            var key = $"{level}|{category ?? ""}|{message}";
            var now = DateTime.Now;

            // Check if we have this exact log recently (within consolidation window)
            if (recentLogs.TryGetValue(key, out var recentLog))
            {
                var timeSinceLastOccurrence = now - recentLog.LastOccurrence;

                // If it happened recently, just increment the counter
                if (timeSinceLastOccurrence <= consolidationWindow)
                {
                    recentLog.Count++;
                    recentLog.LastOccurrence = now;
                    return; // Don't add a new entry
                }
                else
                {
                    // Too old, this is a new burst
                    recentLogs.Remove(key);
                }
            }

            // Check if the LAST log entry is identical (for consecutive duplicates)
            if (logs.Count > 0)
            {
                var lastLog = logs[logs.Count - 1];

                if (lastLog.GetKey() == key)
                {
                    lastLog.Count++;
                    lastLog.LastOccurrence = now;
                    recentLogs[key] = lastLog; // Update recent tracker
                    return;
                }
            }

            // New unique entry
            var newLog = new LogEntry(level, message, category);
            logs.Add(newLog);
            recentLogs[key] = newLog;

            // Cleanup old entries from recent logs dictionary (older than window)
            CleanupRecentLogs(now);

            // Prune old logs
            if (logs.Count > maxLogCount)
            {
                logs.RemoveAt(0);
            }
        }

        private static void CleanupRecentLogs(DateTime now)
        {
            // Remove entries older than the consolidation window
            var keysToRemove = recentLogs
                .Where(kvp => (now - kvp.Value.LastOccurrence) > consolidationWindow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                recentLogs.Remove(key);
            }
        }

        public static void Verbose(string message, string? category = null) => Log(LogLevel.Verbose, message, category);
        public static void Debug(string message, string? category = null) => Log(LogLevel.Debug, message, category);
        public static void Info(string message, string? category = null) => Log(LogLevel.Info, message, category);
        public static void Warning(string message, string? category = null) => Log(LogLevel.Warning, message, category);
        public static void Error(string message, string? category = null) => Log(LogLevel.Error, message, category);

        public static void Clear()
        {
            logs.Clear();
            recentLogs.Clear();
        }

        public static void CopyToClipboard()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var log in logs)
            {
                var countSuffix = log.Count > 1 ? $" (x{log.Count})" : "";
                var timeRange = log.Count > 1
                    ? $"{log.Timestamp:HH:mm:ss} - {log.LastOccurrence:HH:mm:ss}"
                    : $"{log.Timestamp:HH:mm:ss}";

                sb.AppendLine($"[{timeRange}] [{log.Level}] {(log.Category != null ? $"[{log.Category}] " : "")}{log.Message}{countSuffix}");
            }
            ImGui.SetClipboardText(sb.ToString());
        }
    }
}
