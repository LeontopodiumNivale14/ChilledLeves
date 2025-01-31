using Dalamud.Plugin.Services;

namespace ChilledLeves
{
    internal class Service
    {
        internal static Config Configuration { get; set; } = null!;
        internal static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
    }
}
