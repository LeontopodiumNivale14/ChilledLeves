using ChilledLeves.Config_Files;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace ChilledLeves
{
    internal class Service
    {
        internal static Config Config { get; set; } = null!;
        internal static IGameObject gameObject { get; private set; } = null!;
        public static IObjectTable ObjectTable { get; private set; } = null!;
    }
}
