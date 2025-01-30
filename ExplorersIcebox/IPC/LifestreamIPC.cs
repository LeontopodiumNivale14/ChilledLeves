using ECommons.EzIpcManager;

#pragma warning disable CS8618

namespace ExplorersIcebox.IPC
{
    public class LifestreamIPC
    {
        public const string Name = "Lifestream";
        public const string Repo = "https://github.com/NightmareXIV/MyDalamudPlugins/raw/main/pluginmaster.json";
        public LifestreamIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);

        [EzIPC] public Func<string, bool> AethernetTeleport;
        [EzIPC] public Func<uint, byte, bool> Teleport;
        [EzIPC] public Func<bool> TeleportToHome;
        [EzIPC] public Func<bool> TeleportToFC;
        [EzIPC] public Func<bool> TeleportToApartment;
        [EzIPC] public Func<bool> IsBusy;
        [EzIPC] public Action<string> ExecuteCommand;
    }
}
