using ECommons.Automation.NeoTaskManager;
using ECommons.Configuration;
using ChilledLeves.Scheduler;
using ChilledLeves.Ui;
using ChilledLeves.IPC;
using ChilledLeves.Scheduler.Handlers;
using ChilledLeves.Utilities;

namespace ChilledLeves;

public sealed class ChilledLeves : IDalamudPlugin
{
    public string Name => "ChilledLeves";
    internal static ChilledLeves P = null!;
    public static Config C => P.config;
    private Config config;

    // Window's that I use, base window to the settings... need these to actually show shit 
    internal WindowSystem windowSystem;
    internal MainWindow mainWindow;
    internal SettingsWindow settingWindow;
    internal DebugWindow debugWindow;
    internal WorkListUi workListUi;
    internal GatherModeUi gatherModeUi;

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
        mainWindow = new();
        settingWindow = new();
        debugWindow = new();
        workListUi = new();
        gatherModeUi = new();

        taskManager = new(new(abortOnTimeout: true, timeLimitMS: 20000, showDebug: true));
        Svc.PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        Svc.PluginInterface.UiBuilder.OpenMainUi += () =>
        {
            mainWindow.IsOpen = true;
        };
        Svc.PluginInterface.UiBuilder.OpenConfigUi += () =>
        {
            workListUi.IsOpen = true;
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

    public void Dispose()
    {
        Safe(() => Svc.Framework.Update -= Tick);
        Safe(() => Svc.PluginInterface.UiBuilder.Draw -= windowSystem.Draw);
        ECommonsMain.Dispose();
        Safe(TextAdvancedManager.UnlockTA);
        Safe(YesAlreadyManager.Unlock);
    }

    private void OnCommand(string command, string args)
    {
        var subcommands = args.Split(' ');

        if (subcommands.Length == 0 || args == "")
        {
            mainWindow.IsOpen = !mainWindow.IsOpen;
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
            workListUi.IsOpen = true;
            return;
        }
        else if (firstArg.ToLower() == "add")
        {
            string secondCommand = subcommands[1];
            string thirdCommand = subcommands[2];
            uint leveId = 0;
            int repeatAmount = 0;
            if (int.TryParse(secondCommand, out int value) && LeveDictionary.ContainsKey((uint)value))
            {
                leveId = (uint)value;
                if (int.TryParse(thirdCommand, out int repeat) && (repeat > 0 && repeat <= 100))
                {
                    repeatAmount = repeat;
                    if (!C.workList.Any(e => e.LeveID == leveId))
                    {
                        C.workList.Add(new LeveEntry { LeveID = leveId, InputValue = repeatAmount});
                        C.Save();
                        return;
                    }
                }
                else
                {
                    PluginVerbos($"{repeat} is not a valud input for the amount that you would like to do. Please input between 1-100");
                    return;
                }
            }
            else
            {
                PluginVerbos($"The leve you tried adding isn't a valid leveID: {subcommands[1]}");
                return;
            }
        }
        else if (firstArg.ToLower() == "clear")
        {
            C.workList.Clear();
            C.Save();
            PluginVerbos("Cleared the worklist of all leves");
            return;
        }
        else if (firstArg.ToLower() == "start")
        {
            SchedulerMain.EnablePlugin();
            PluginVerbos("Starting the turnin process");
            return;
        }
        else if (firstArg.ToLower() == "stop")
        {
            SchedulerMain.DisablePlugin();
            PluginVerbos("Sopping the turnin process");
            return;
        }
        else
        {
            PluginVerbos($"Length of array is: {subcommands.Length} & no matching description. Can't do command");
            PluginVerbos($"Command: -{command}- args?:-{args}-");
            return;
        }
    }
}
