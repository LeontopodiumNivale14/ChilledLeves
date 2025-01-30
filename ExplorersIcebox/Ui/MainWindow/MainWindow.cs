using ExplorersIcebox.Scheduler;

namespace ExplorersIcebox.Ui.MainWindow;

internal class MainWindow : Window
{
    public MainWindow() :
        base($"Explorer's Icebox {P.GetType().Assembly.GetName().Version}")
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

    private const uint SidebarWidth = 275;
    public void Dispose() { }

    public static string currentlyDoing = SchedulerMain.CurrentProcess;
    private bool copyButton = false;

    public override void Draw()
    {
        if (IPC.NavmeshIPC.Installed && IPC.VislandIPC.Installed)
        {
            ImGuiEx.EzTabBar("EIB tabbar",
                            ("XPGrind/Item Gathering", GrindModeUi.GrindModeUi.Draw, null, true),
                            ("Help", HelpUi.Draw, null, true),
                            ("Version Notes", VersionNotesUi.Draw, null, true),
                            ("About", About.Draw, null, true)
                            );
        }
        else if (!IPC.NavmeshIPC.Installed
                || !IPC.VislandIPC.Installed)
        {
            string plugins = "";
            if (!IPC.NavmeshIPC.Installed && !IPC.VislandIPC.Installed)
                plugins = "Navmesh & Visland";
            else if (!IPC.VislandIPC.Installed)
                plugins = "Visland";
            else if (!IPC.NavmeshIPC.Installed)
                plugins = "Vnavmesh";

            FancyCheckmark(IPC.NavmeshIPC.Installed);
            ImGui.SameLine();
            ImGui.Text("Navmesh Installed");

            FancyCheckmark(IPC.VislandIPC.Installed);
            ImGui.SameLine();
            ImGui.Text("Visland Installed");

            ImGui.Text($"You seem to be missing {plugins}, if you need the repo, you can press the button and it'll copy to the clipboard");
            if (ImGui.Button(copyButton ? "Copied Successfully" : "Copy Repo Link"))
            {
                ImGui.SetClipboardText("https://puni.sh/api/repository/veyn");
                copyButton = true;
            }
        }

    }
}
