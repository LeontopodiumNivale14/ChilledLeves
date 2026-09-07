using ChilledLeves.Utilities;
using ECommons.EzIpcManager;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ChilledLeves.IPC;

public class NavmeshIPC
{
    public const string Name = "vnavmesh";
    public const string Repo = "https://puni.sh/api/repository/veyn";
    public NavmeshIPC() => EzIPC.Init(this, Name);
    public bool Installed => Utils.HasPlugin(Name);

    // Nav
    [EzIPC("Nav.%m")] public readonly Func<bool> IsReady;
    [EzIPC("Nav.%m")] public readonly Func<float> BuildProgress;
    [EzIPC("Nav.%m")] public readonly Func<bool> Reload;
    [EzIPC("Nav.%m")] public readonly Func<bool> Rebuild;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, Task<List<Vector3>>> Pathfind;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, float, Task<List<Vector3>>> PathfindWithTolerance;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, Vector3, float, Task<List<Vector3>>> PathfindAvoid;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, Vector3, bool, CancellationToken, Task<List<Vector3>>> PathfindCancelable;
    [EzIPC("Nav.%m")] public readonly Action PathfindCancelAll;
    [EzIPC("Nav.%m")] public readonly Func<bool> PathfindInProgress;
    [EzIPC("Nav.%m")] public readonly Func<int> PathfindNumQueued;
    [EzIPC("Nav.%m")] public readonly Func<bool> IsAutoLoad;
    [EzIPC("Nav.%m")] public readonly Action<bool> SetAutoLoad;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, string, float, bool> BuildBitmap;
    [EzIPC("Nav.%m")] public readonly Func<Vector3, string, float, Vector3, Vector3, bool> BuildBitmapBounded;
    [EzIPC("Nav.%m")] public readonly Func<List<Vector3>, string, float, bool> BuildBitmapMulti;
    [EzIPC("Nav.%m")] public readonly Func<List<Vector3>, string, float, Vector3, Vector3, bool> BuildBitmapMultiBounded;

    // Query.Mesh
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3?> NearestPoint;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, bool, bool> IsPointOnMesh;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, float, float, Vector3?> NearestPointReachable;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3, bool, float, Vector3?> PointOnFloor;
    [EzIPC("Query.Mesh.%m")] public readonly Func<Vector3?> FlagToPoint;

    // Path
    [EzIPC("Path.%m")] public readonly Action<List<Vector3>, bool> MoveTo;
    [EzIPC("Path.Stop")] public readonly Action PathStop;
    [EzIPC("Path.%m")] public readonly Func<bool> IsRunning;
    [EzIPC("Path.%m")] public readonly Func<int> NumWaypoints;
    [EzIPC("Path.%m")] public readonly Func<List<Vector3>> ListWaypoints;
    [EzIPC("Path.%m")] public readonly Func<bool> GetMovementAllowed;
    [EzIPC("Path.%m")] public readonly Action<bool> SetMovementAllowed;
    [EzIPC("Path.%m")] public readonly Func<bool> GetAlignCamera;
    [EzIPC("Path.%m")] public readonly Action<bool> SetAlignCamera;
    [EzIPC("Path.%m")] public readonly Func<float> GetTolerance;
    [EzIPC("Path.%m")] public readonly Action<float> SetTolerance;

    // SimpleMove
    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, bool> PathfindAndMoveTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<Vector3, bool, float, bool> PathfindAndMoveCloseTo;
    [EzIPC("SimpleMove.%m")] public readonly Func<bool> PathfindInProgress_Simple;

    // Window
    [EzIPC("Window.%m")] public readonly Func<bool> IsOpen;
    [EzIPC("Window.%m")] public readonly Action<bool> SetOpen;

    // DTR
    [EzIPC("DTR.%m")] public readonly Func<bool> IsShown;
    [EzIPC("DTR.%m")] public readonly Action<bool> SetShown;

    // Don't use these rn
    [EzIPC("SmartNav.%m")] public readonly Action PathToFlag;
    [EzIPC("SmartNav.Stop")] public readonly Action SmartNavStop;
    [EzIPC("SmartNav.IsRunning")] public readonly Func<bool> SmartIsRunning;
    [EzIPC("SmartNav.PathToPoint")] public readonly Action<uint, Vector3> Smart_PathToPoint;
    [EzIPC("SmartNav.PathToTerritory")] public readonly Action<uint> Smart_PathToTerritory;

    public void Stop()
    {
        if (Installed)
        {
            if (IsRunning())
                PathStop();
        }
    }

    public bool NavRunning()
    {
        return IsRunning();
    }

    public void SmartPath(uint territory, Vector3? position = null)
    {
        if (position != null)
            P.navmesh.Smart_PathToPoint(territory, position.Value);
        else
            P.navmesh.Smart_PathToTerritory(territory);
    }
}