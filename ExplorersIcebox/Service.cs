using Dalamud.Plugin.Services;

namespace ExplorersIcebox
{
    internal class Service
    {
        internal static ExplorersIcebox IsLeveling { get; set; } = null!;
        internal static Config Configuration { get; set; } = null!;
        internal static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
    }
}
