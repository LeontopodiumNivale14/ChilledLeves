using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace ChilledLeves
{
    internal class Service
    {
        internal static Config Config { get; set; } = null!;
        internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        internal static ITextureProvider TextureDal { get; set; } = null!;
        internal static IGameObject gameObject { get; private set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
    }
}
