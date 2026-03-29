using ChilledLeves.Utilities;
using ECommons.EzIpcManager;
using System.Collections.Generic;

namespace ChilledLeves.IPC;

public class NavmeshIPC
{
    public const string Name = "vnavmesh";
    public const string Repo = "https://puni.sh/api/repository/veyn";
    public NavmeshIPC() => EzIPC.Init(this, Name);
    public bool Installed => Utils.HasPlugin(Name);

    [EzIPC("Nav.%m")] public readonly Func<bool> IsReady;
    [EzIPC("Nav.%m")] public readonly Func<float> BuildProgress;
    [EzIPC("Nav.%m")] public readonly Func<bool> Reload;
    [EzIPC("Nav.%m")] public readonly Func<bool> Rebuild;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, Vector3> Pathfind;

    /// <summary>
    /// Pathfind then move across the path it gives <br></br>
    /// Vector3 = Position <br></br>
    /// Bool = Fly <br></br>
    /// </summary>
    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, bool> PathfindAndMoveTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<bool> PathfindInProgress;

    [EzIPC("Path.%m")] public readonly Action<List<Vector3>, bool> MoveTo;
    [EzIPC("Path.Stop")] public readonly Action PathStop;
    [EzIPC("Path.%m")] public readonly Action<bool> SetAlignCamera;
    [EzIPC("Path.%m")] public readonly Func<bool> IsRunning;
    [EzIPC("Path.%m")] public readonly Action<float> SetTolerance;

    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3?> NearestPoint;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, bool, float, Vector3?> PointOnFloor;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3?> NearestPointReachable;

    [EzIPC("SmartNav.%m")] public readonly Action PathToFlag;
    [EzIPC("SmartNav.Stop")] public readonly Action SmartNavStop;
    [EzIPC("SmartNav.IsRunning")] public readonly Func<bool> SmartIsRunning;
    [EzIPC("SmartNav.PathToPoint")] public readonly Action<uint, Vector3> Smart_PathToPoint;
    [EzIPC("SmartNav.PathToTerritory")] public readonly Action<uint> Smart_PathToTerritory;

    public bool NavRunning()
    {
        return SmartIsRunning() || IsRunning();
    }

    public void SmartPath(uint territory, Vector3? position = null)
    {
        if (position != null)
            P.navmesh.Smart_PathToPoint(territory, position.Value);
        else
            P.navmesh.Smart_PathToTerritory(territory);
    }
}