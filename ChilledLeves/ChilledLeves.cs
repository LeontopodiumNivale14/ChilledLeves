using ChilledLeves.Config_Files;
using ChilledLeves.IPC;
using ChilledLeves.Resources;
using ChilledLeves.Scheduler;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Ui;
using ChilledLeves.Ui.Old_Ui;
using ChilledLeves.Utilities;
using ChilledLeves.Utilities.LeveData;
using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;
using ECommons.GameHelpers;
using Pictomancy;

namespace ChilledLeves;

public sealed class ChilledLeves : IDalamudPlugin
{
    public string Name => "ChilledLeves";
    internal static ChilledLeves P = null!;
    public static Config C => P.config;
    private Config config;

    // Window's that I use, base window to the settings... need these to actually show shit 
    internal WindowSystem windowSystem;
    internal Window_Debug debugWindow;
    internal AlertWindow alertUi; 
    internal AlertSettings alertSettings;

    internal Window_Main window_Main;

    // Taskmanager from Ecommons
    internal TaskManager taskManager;

    // Internal IPC's that I use for... well plugins. 
    internal LifestreamIPC lifestream;
    internal NavmeshIPC navmesh;
    internal PandoraIPC pandora;
    internal ArtisanIPC artisan;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ChilledLeves(IDalamudPluginInterface pi)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        P = this;
        ECommonsMain.Init(pi, P, ECommons.Module.DalamudReflector, ECommons.Module.ObjectFunctions);
        new ECommons.Schedulers.TickScheduler(Load);
        PictoService.Initialize(pi);
    }

    public void Load()
    {
        EzConfig.Migrate<Config>();
        config = EzConfig.Init<Config>();

        //IPC's that are used
        taskManager = new();
        lifestream = new();
        navmesh = new();
        pandora = new();
        artisan = new();

        // all the windows
        windowSystem = new();
        debugWindow = new();
        alertUi = new();
        alertSettings = new();

        window_Main = new();

        taskManager = new(new(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));
        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.Draw += OnDraw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            window_Main.IsOpen = true;
        };
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {

        };
        EzCmd.Add("/chilledleves", OnCommand, """
            Open plugin interface
            /chilledleves add [leveID] [amount] - adds the leveID/amount to worklist
            /chilledleves clear - clears the worklist 
            /chilledleves start | stop - starts/stops the turnin process
            /chilledleves s|settings - Opens the worklist menu
            /leveitalone - alias
            """);
        EzCmd.Add("/leveitalone", OnCommand);
        Svc.Framework.Update += Tick;

        ExcelHelper.Init();
        Config_Migrate.UpdateConfig();
        LeveInfo.PopulateLeveInfo();
        LeveInfo.UpdateLeves();
        RouteLoader.LoadAllRoutes();
    }

    private void Tick(object _)
    {
        if (SchedulerMain.AreWeTicking && Svc.ClientState.LocalPlayer != null)
        {
            SchedulerMain.Tick();
        }
        GenericManager.Tick();
        TextAdvancedManager.Tick();
        YesAlreadyManager.Tick();
        SoundAlert.Tick();
    }

    public void OnDraw()
    {
        if (Player.Available)
            PictoManager.DrawPicto();
    }

    public void Dispose()
    {
        Safe(() => Svc.Framework.Update -= Tick);
        Safe(() => Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw);
        Safe(() => Svc.PluginInterface.UiBuilder.Draw -= OnDraw);
        ECommonsMain.Dispose();
        Safe(TextAdvancedManager.UnlockTA);
        Safe(YesAlreadyManager.Unlock);
        PictoService.Dispose();
    }

    private void OnCommand(string command, string args)
    {
        var subcommands = args.Split(' ');

        if (subcommands.Length == 0 || args == "")
        {
            window_Main.IsOpen = !window_Main.IsOpen;
            return;
        }

        var firstArg = subcommands[0];

        if (firstArg.ToLower() == "d" || firstArg.ToLower() == "debug")
        {
            debugWindow.IsOpen = true;
            return;
        }
        else if (firstArg.ToLower() == "s" || firstArg.ToLower() == "settings")
        {

            return;
        }
        else if (firstArg.ToLower() == "add")
        {

        }
        else if (firstArg.ToLower() == "clear")
        {

            return;
        }
        else if (firstArg.ToLower() == "start")
        {

            return;
        }
        else if (firstArg.ToLower() == "stop")
        {

            return;
        }
        else if (firstArg.ToLower() == "test")
        {
            window_Main.IsOpen = true;
        }
        else
        {
            PluginVerbos($"Length of array is: {subcommands.Length} & no matching description. Can't do command");
            PluginVerbos($"Command: -{command}- args?:-{args}-");
            return;
        }
    }
}
