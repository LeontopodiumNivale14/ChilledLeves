using ECommons.Automation.NeoTaskManager;
using ECommons.Reflection;
using System.Reflection.Metadata;

namespace ChilledLeves.Utilities.Utils
{
    public static partial class Utils
    {
        // Plugin | Ecommon Infomation
        public static bool HasPlugin(string name) => DalamudReflector.TryGetDalamudPlugin(name, out _, false, true);
        public static void PluginVerbos(string m, string handle = null)
        {
            if (handle != null)
                ECommons.Logging.PluginLog.Verbose($"[{handle}] {m}");
            else
                ECommons.Logging.PluginLog.Verbose($"{m}");
        }
        public static void PluginInfo(string m, string handle = null)
        {
            if (handle != null)
                ECommons.Logging.PluginLog.Information($"[{handle}] {m}");
            else
                ECommons.Logging.PluginLog.Information($"{m}");
        }
        public static void PluginDebug(string m, string handle = null)
        {
            if (handle != null)
                ECommons.Logging.PluginLog.Debug($"[{handle}] {m}");
            else
                ECommons.Logging.PluginLog.Debug($"{m}");
        }
        public static void PluginError(string m, string handle = null)
        {
            if (handle != null)
                ECommons.Logging.PluginLog.Error($"[{handle}] {m}");
            else
                ECommons.Logging.PluginLog.Error($"{m}");
        }
        public static TaskManagerConfiguration DConfig => new(timeLimitMS: 10 * 60 * 3000, abortOnTimeout: false);
    }
}
