using ECommons.EzIpcManager;

namespace ChilledLeves.IPC;

#nullable disable
public class PandoraIPC
{
    public const string Name = "PandorasBox";
    public const string Repo = "https://love.puni.sh/ment.json";

    public PandoraIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);
    public bool Installed => HasPlugin(Name);

    [EzIPC] public Func<string, bool?> GetFeatureEnabled;
    [EzIPC] public Func<string, string, bool?> GetConfigEnabled;
}
