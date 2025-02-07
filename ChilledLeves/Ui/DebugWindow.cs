using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.Automation.LegacyTaskManager;
using ChilledLeves.Scheduler.Tasks;
using System.Collections.Generic;
using System.IO;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.VertexShader;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Globalization;
using Dalamud.Game.ClientState.Objects.Types;

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
        ImGuiEx.EzTabBar("ROR Debug Tabs",
                ("Main Debug###LeaveItAloneMainDebug", MainDebug, null, true),
                ("Targeting Debug ###LeveItAloneTargeting", TargetingDebug, null, true)
        );

    }

    private void MainDebug()
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

    private static float TargetXPos = 0;
    private static float TargetYPos = 0;
    private static float TargetZPos = 0;
    private static IGameObject? GameObject = null;
    private static uint TargetDataID = 0;
    private static string TargetDataIDString = TargetDataID.ToString();

    public void TargetingDebug()
    {
        if (Svc.Targets?.Target != null)
        {
            TargetXPos = (float)Math.Round(Svc.Targets.Target.Position.X, 2);
            TargetYPos = (float)Math.Round(Svc.Targets.Target.Position.Y, 2);
            TargetZPos = (float)Math.Round(Svc.Targets.Target.Position.Z, 2);
            // Get the GameObjectId and display it in the ImGui.Text box
            ImGui.Text($"Name: {Svc.Targets.Target.Name}");
            ImGui.Text($"GameObjectId: {Svc.Targets.Target.GameObjectId}");
            ImGui.Text($"DataID: {Svc.Targets.Target.DataId}");
            if (ImGui.Button("Copy DataID to clipboard"))
            {
                ImGui.SetClipboardText($"{Svc.Targets.Target.DataId}");
            }
            if (ImGui.Button("Copy GameObjectID to clipboard"))
            {
                ImGui.SetClipboardText($"{Svc.Targets.Target.GameObjectId}");
            }
            ImGui.Text($"Target Pos: X: {TargetXPos}, Y: {TargetYPos}, Z: {TargetZPos}");
            if (ImGui.Button("Copy Target XYZ"))
            {
                ImGui.SetClipboardText($"{TargetXPos.ToString("0.00", CultureInfo.InvariantCulture)}f, " +
                                       $"{TargetYPos.ToString("0.00", CultureInfo.InvariantCulture)}f, " +
                                       $"{TargetZPos.ToString("0.00", CultureInfo.InvariantCulture)}f");
            }
        }
        else
        {
            // Optionally display a message if no target is selected
            ImGui.Text("No target selected.");
        }
    }
}
