using ECommons.EzIpcManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorersIcebox.IPC;

#nullable disable
public class VislandIPC
{
    public const string Name = "visland";
    public VislandIPC() => EzIPC.Init(this, Name, SafeWrapper.AnyException);
    public static bool Installed => HasPlugin(Name);

    [EzIPC] public Func<bool> IsRouteRunning; // Checks to see if visland is running
    [EzIPC] public Func<bool> IsRoutePaused; // Checks to see if route is paused
    [EzIPC] public Action<bool> SetRoutePaused; // Bool to set visland route paused/not
    [EzIPC] public Action StopRoute; // Run this to stop a route from continuing
    [EzIPC] public Action<string, bool> StartRoute; // string = base64 import | bool is if you only want to run it once
    [EzIPC] public Action<string[], bool> VIsMoveTo;
}
