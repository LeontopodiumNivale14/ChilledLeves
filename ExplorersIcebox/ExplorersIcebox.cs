using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;
using ExplorersIcebox.IPC;
using ExplorersIcebox.Scheduler;
using ExplorersIcebox.Ui;
using ExplorersIcebox.Ui.MainWindow;

namespace ExplorersIcebox;

public sealed class ExplorersIcebox : IDalamudPlugin
{
    public string Name => "ExplorersIcebox";
    internal static ExplorersIcebox P = null!;
    public static Config C => P.config;
    private Config config;

    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;
    internal DebugWindow debugWindow;
    internal SettingWindow settingWindow;

    internal TaskManager taskManager;


    internal LifestreamIPC lifestream;
    internal NavmeshIPC navmesh;
    internal VislandIPC visland;

    public ExplorersIcebox(IDalamudPluginInterface pi)
    {
        //Test.(
        P = this;
        ECommonsMain.Init(pi, P, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        new ECommons.Schedulers.TickScheduler(Load);
    }

    public void Load()
    {
        EzConfig.Migrate<Config>();
        config = EzConfig.Init<Config>();

        //IPC's that are used
        taskManager = new();
        lifestream = new();
        navmesh = new();
        visland = new();

        // all the windows
        windowSystem = new();
        mainWindow = new();
        settingWindow = new();
        debugWindow = new();

        taskManager = new(new(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));
        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
        };
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            settingWindow.IsOpen = true;
        };
        EzCmd.Add("/explorersicebox", OnCommand, """
            Open plugin interface
            /icebox - alias for /explorersicebox
            /explorersicebox s|settings - Opens the workshop menu
            """);
        EzCmd.Add("/icebox", OnCommand);
        Svc.Framework.Update += Tick;
    }

    private void Tick(object _)
    {
        if (SchedulerMain.AreWeTicking && Svc.ClientState.LocalPlayer != null)
        {
            SchedulerMain.Tick();
        }
    }

    public void Dispose()
    {
        Safe(() => Svc.Framework.Update -= Tick);
        Safe(() => Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw);
        ECommonsMain.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        if (args.EqualsIgnoreCaseAny("d", "debug"))
        {
            debugWindow.IsOpen = !debugWindow.IsOpen;
        }
        else if (args.EqualsIgnoreCaseAny("s", "settings"))
        {
            settingWindow.IsOpen = !settingWindow.IsOpen;
        }
        else
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
        }
    }
}
