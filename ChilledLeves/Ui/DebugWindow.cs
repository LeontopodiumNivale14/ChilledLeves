using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.Automation.LegacyTaskManager;
using ChilledLeves.Scheduler.Tasks;
using System.Collections.Generic;
using System.IO;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.VertexShader;
using Lumina.Excel.Sheets;

namespace ChilledLeves.Ui;

internal class DebugWindow : Window
{
    public DebugWindow() :
        base($"Chilled LevesTableOld Debug {P.GetType().Assembly.GetName().Version} ###ChilledLevesDebug")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(800, 1200)
        };
        P.windowSystem.AddWindow(this);
    }

    public void Dispose() { }

    // variables that hold the "ref"s for ImGui

    public override void Draw()
    {
        ImGui.Text($"Miner: {C.LeveFilter.ShowMiner}");
        ImGui.Text($"Botanist: {C.LeveFilter.ShowBotanist}");
        ImGui.Text($"Fisher: {C.LeveFilter.ShowFisher}");
        ImGui.Text($"Carpenter: {C.LeveFilter.ShowCarpenter}");
        ImGui.Text($"Blacksmith: {C.LeveFilter.ShowBlacksmith}");
        ImGui.Text($"Armorer: {C.LeveFilter.ShowArmorer}");
        ImGui.Text($"Goldsmith: {C.LeveFilter.ShowGoldsmith}");
        ImGui.Text($"Leatherworker {C.LeveFilter.ShowLeatherworker}");
        ImGui.Text($"Weaver: {C.LeveFilter.ShowWeaver}");
        ImGui.Text($"Alchemist: {C.LeveFilter.ShowAlchemist}");
        ImGui.Text($"Culinarian: {C.LeveFilter.ShowCulinarian}");

        foreach (var entry in C.workList)
        {
            uint leve = entry.LeveID;
            ImGui.PushID((int)leve);
            string leveName = LeveDict[leve].LeveName;


            ImGui.SetNextItemWidth(75);
            int inputValue = entry.InputValue;
            ImGui.InputInt($"###LeveID Input {leve}", ref inputValue);
            entry.InputValue = inputValue;
            ImGui.SameLine();
            ImGui.Text($"Leve: {leveName} | Dict Value: {entry.InputValue}");
        }

    }
}
