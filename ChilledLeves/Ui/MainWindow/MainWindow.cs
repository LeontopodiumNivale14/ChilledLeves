using ChilledLeves.Scheduler;
using ChilledLeves.Util;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using static Lumina.Data.Parsing.Uld.UldRoot;

namespace ChilledLeves.Ui.MainWindow;

internal class MainWindow : Window
{
    public MainWindow() :
        base($"Chilled LevesTableOld {P.GetType().Assembly.GetName().Version} ###ChilledLevesMainWindow")
    {
        Flags = ImGuiWindowFlags.None;
        SizeConstraints = new()
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(2000, 2000)
        };
        P.windowSystem.AddWindow(this);
        AllowPinning = false;

        PopulateDictionary();
    }

    public void Dispose() { }

    public static string currentlyDoing = SchedulerMain.CurrentProcess;

    public override void Draw()
    {
        UpdateItemAmount();

        DrawFilterPanel();
        TestSheet.Draw();
    }

    private void DrawFilterPanel()
    {
        DrawButton(LeveTypeDict[5].AssignmentIcon, $"{LeveTypeDict[5].LeveClassType} Leves", ref C.LeveFilter.ShowCarpenter, true);
        DrawButton(LeveTypeDict[6].AssignmentIcon, $"{LeveTypeDict[6].LeveClassType} Leves", ref C.LeveFilter.ShowBlacksmith, true);
        DrawButton(LeveTypeDict[7].AssignmentIcon, $"{LeveTypeDict[7].LeveClassType} Leves", ref C.LeveFilter.ShowArmorer, true);
        DrawButton(LeveTypeDict[8].AssignmentIcon, $"{LeveTypeDict[8].LeveClassType} Leves", ref C.LeveFilter.ShowGoldsmith, true);
        DrawButton(LeveTypeDict[9].AssignmentIcon, $"{LeveTypeDict[9].LeveClassType} Leves", ref C.LeveFilter.ShowLeatherworker, true);
        DrawButton(LeveTypeDict[10].AssignmentIcon, $"{LeveTypeDict[10].LeveClassType} Leves", ref C.LeveFilter.ShowWeaver, true);
        DrawButton(LeveTypeDict[11].AssignmentIcon, $"{LeveTypeDict[11].LeveClassType} Leves", ref C.LeveFilter.ShowAlchemist, true);
        DrawButton(LeveTypeDict[12].AssignmentIcon, $"{LeveTypeDict[12].LeveClassType} Leves", ref C.LeveFilter.ShowCulinarian, false);
    }

    private void DrawButton(ISharedImmediateTexture? icon, string tooltip, ref bool state, bool sameLine)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, new Vector2(24)))
        {
            state = !state;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(tooltip);
        if (sameLine)
            ImGui.SameLine();
    }
}
