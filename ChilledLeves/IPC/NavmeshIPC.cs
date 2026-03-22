using ECommons.EzIpcManager;
using System.Collections.Generic;

namespace ChilledLeves.IPC;

public class NavmeshIPC
{
    public const string Name = "vnavmesh";
    public const string Repo = "https://puni.sh/api/repository/veyn";
    public NavmeshIPC() => EzIPC.Init(this, Name);
    public static bool Installed => HasPlugin(Name);

    [EzIPC("Nav.%m")] public readonly Func<bool> IsReady;
    [EzIPC("Nav.%m")] public readonly Func<float> BuildProgress;
    [EzIPC("Nav.%m")] public readonly Func<bool> Reload;
    [EzIPC("Nav.%m")] public readonly Func<bool> Rebuild;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, Vector3> Pathfind;

    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, bool> PathfindAndMoveTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<bool> PathfindInProgress;

    [EzIPC("Path.%m")] public readonly Action<List<Vector3>, bool> MoveTo;
    [EzIPC("Path.Stop")] public readonly Action PathStop;
    [EzIPC("Path.%m")] public readonly Action<bool> SetAlignCamera;
    [EzIPC("Path.%m")] public readonly Func<bool> IsRunning;

    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3?> NearestPoint;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, bool, float, Vector3?> PointOnFloor;

    [EzIPC("SmartNav.%m")] public readonly Action PathToFlag;
    [EzIPC("SmartNav.Stop")] public readonly Action SmartNavStop;
    [EzIPC("SmartNav.IsRunning")] public readonly Func<bool> SmartIsRunning;
    [EzIPC("SmartNav.PathTo")] public readonly Action<uint, Vector3> SmartPathTo;
}