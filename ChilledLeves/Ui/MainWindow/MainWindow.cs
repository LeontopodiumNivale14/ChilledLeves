using ChilledLeves.Scheduler;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

namespace ChilledLeves.Ui.MainWindow;

internal class MainWindow : Window
{
    public MainWindow() :
        base($"Chilled Leves {P.GetType().Assembly.GetName().Version} ###ChilledLevesMainWindow")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(2000, 2000)
        };
        P.windowSystem.AddWindow(this);
        AllowPinning = false;
    }

    public void Dispose() { }

    public static string currentlyDoing = SchedulerMain.CurrentProcess;

    public override void Draw()
    {
        if (IPC.NavmeshIPC.Installed && IPC.VislandIPC.Installed)
        {
            ImGuiEx.EzTabBar("EIB tabbar",
                            ("XPGrind/Item Gathering", CraftingLeves.Draw, null, true),
                            ("Help", HelpUi.Draw, null, true),
                            ("Version Notes", VersionNotesUi.Draw, null, true),
                            ("About", About.Draw, null, true)
                            );
        }

    }
}
