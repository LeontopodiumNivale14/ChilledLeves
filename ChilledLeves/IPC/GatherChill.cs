using ChilledLeves.Utilities;
using ECommons.EzIpcManager;

namespace ChilledLeves.IPC;

public class GatherChill
{
    public const string Name = "GatherChill";
    public const string Repo = "https://puni.sh/api/repository/ice";

    /// <summary>
    /// Lowest GatherChill API version this plugin works with.
    /// </summary>
    private const int MinApiVersion = 1;

    public GatherChill() => EzIPC.Init(this, Name);

    public bool Installed => Utils.HasPlugin(Name);

    [EzIPC] public readonly Func<int> ApiVersion;
    [EzIPC] public readonly Func<bool> IsBusy;
    [EzIPC] public readonly Func<uint, bool> CanGather;
    [EzIPC] public readonly Func<(uint ItemId, int Amount)[], int> AddToGatherList;

    /// <summary>
    /// True if GatherChill is loaded and its API is new enough for us.
    /// Check this before calling anything else.
    /// </summary>
    public bool IsReady()
    {
        if (!Installed)
            return false;

        try
        {
            // Read as: "provider's version is at least the version I need"
            return ApiVersion() >= MinApiVersion;
        }
        catch (Exception)
        {
            // Plugin is installed but IPC isn't registered yet (still loading, or disabled).
            return false;
        }
    }

    /// <summary>
    /// Sends items to GatherChill, skipping anything it can't gather.
    /// Returns how many items were added or updated, or 0 if GatherChill isn't available.
    /// </summary>
    public int TryAddToGatherList((uint ItemId, int Amount)[] items)
    {
        if (items.Length == 0)
            return 0;

        if (!IsReady())
            return 0;

        try
        {
            return AddToGatherList(items);
        }
        catch (Exception)
        {
            return 0;
        }
    }
}