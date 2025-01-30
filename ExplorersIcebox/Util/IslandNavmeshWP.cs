using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ExplorersIcebox.Util;

internal class IslandNavmeshWP
{
    public static readonly Vector3 workshopEntrancePos = new Vector3(-268f, 40f, 226f);
    public static readonly Vector3 mammetExportPos = new Vector3(-266.74f, 41.03f, 209.97f);
    public static readonly Vector3 workshopExitPos = new Vector3(-268.27f, 40f, 232.9f);

    public static readonly Vector3 Clay_SandRoutePos = new Vector3(213.4f, 58.66f, 88.73f);
    public static readonly Vector3 QuartzRoutePos = new Vector3(349.53f, 397.14f, -401.77f);

    // dummy things till I can fully check them in
    public static readonly Vector3 DummmyPos = new Vector3(0, 0, 0);

    public static readonly List<Vector3> testPoints = new List<Vector3>
    {
        new Vector3(183.92f, 14.1f, 666.69f),
        new Vector3(191.9f, 14.1f, 666.12f),
        new Vector3(195.98f, 14.1f, 656.97f),
        new Vector3(208.01f, 14.1f, 666.37f),
    };

    public static readonly List<Vector3> testPoints2 = new List<Vector3>
    {
        new Vector3(178.2f, 14.1f, 675.8f),
        new Vector3(169.79f, 14.1f, 673.17f),
    };

    /*
    public static List<Vector3> Base2Clam = new List<Vector3>
    {

    };
    */

    public static List<Vector3> Base2IslewortVnav = new List<Vector3>
    {
        new Vector3(-260.29f, 40f, 230.69f),
        new Vector3(68.96f, 65.27f, 58.94f),
        new Vector3(83.47f, 66.29f, 31.24f),
        new Vector3(19.02f, 66.64f, -64.17f),
        new Vector3(-177.14f, 57.53f, -414.01f),
    };

    public static List<Vector3> Base2ClamNav = new List<Vector3>
    {
        new Vector3(-268.21f, 40f, 232.05f),
        new Vector3(-140.83f, 112.82f, 131.53f),
        new Vector3(346.42f, 400.38f, -391.87f),
        new Vector3(350.07f, 394.29f, -399.85f),
    };

}
