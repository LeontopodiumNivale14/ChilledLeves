using ECommons.EzIpcManager;

namespace ChilledLeves.IPC;

#nullable disable
public class PandoraIPC
{
    public const string Name = "PandorasBox";
    public const string Repo = "https://love.puni.sh/ment.json";

    public PandoraIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);
    public bool Installed => HasPlugin(Name);

    [EzIPC] public readonly Func<string, bool?> GetFeatureEnabled;
    [EzIPC] public readonly Func<string, string, bool?> GetConfigEnabled;
    [EzIPC] public readonly Action<string, bool?> SetFeatureEnabled;
    [EzIPC] public readonly Action<string, string, bool> SetConfigEnabled;
    [EzIPC] public readonly Action<string, int> PauseFeature;
}
