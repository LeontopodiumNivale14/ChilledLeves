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
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Text.Json;

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
                ("Targeting Debug ###LeveItAloneTargeting", TargetingDebug, null, true),
                ("Temp ###Temp Debug", ListGrabber, null, true),
                ("Place Name Difference", PlaceName, null, true)
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
            string leveName = CrafterLeves[leve].LeveName;


            ImGui.SetNextItemWidth(75);
            int inputValue = entry.InputValue;
            ImGui.InputInt($"###LeveID Input {leve}", ref inputValue);
            entry.InputValue = inputValue;
            ImGui.SameLine();
            ImGui.Text($"Leve: {leveName} | Dict Value: {entry.InputValue}");
        }

        ImGui.Text($"{CurrentMap().ToString()}");

        if (ImGui.Button("Open map to Red Rooster"))
        {
            SetFlagForNPC(16, 135, 32, 19.2f);
        }

        if (ImGui.Button("Export"))
        {
            ExportDict();
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
        uint currentZone = CurrentTerritory();
        if (ImGui.Button("Copy zoneID"))
        {
            ImGui.SetClipboardText($"ZoneID = {currentZone}");
        }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Zone ID: {currentZone}");

        if (ImGui.Button("Copy current POS"))
        {
            ImGui.SetClipboardText($"NPCLocation = new Vector3 ({GetPlayerRawXPos().ToString("0.00", CultureInfo.InvariantCulture)}f, " +
                                   $"{GetPlayerRawYPos().ToString("0.00", CultureInfo.InvariantCulture)}f, " +
                                   $"{GetPlayerRawZPos().ToString("0.00", CultureInfo.InvariantCulture)}f)");
        }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{GetPlayerRawXPos().ToString("0.00", CultureInfo.InvariantCulture)}, " +
                   $"{GetPlayerRawYPos().ToString("0.00", CultureInfo.InvariantCulture)}, " +
                   $"{GetPlayerRawZPos().ToString("0.00", CultureInfo.InvariantCulture)}");

        if (Svc.Targets?.Target != null)
        {

            TargetXPos = (float)Math.Round(Svc.Targets.Target.Position.X, 2);
            TargetYPos = (float)Math.Round(Svc.Targets.Target.Position.Y, 2);
            TargetZPos = (float)Math.Round(Svc.Targets.Target.Position.Z, 2);
            // Get the GameObjectId and display it in the ImGui.Text box
            ImGui.Text($"Name: {Svc.Targets.Target.Name}");
            ImGui.Text($"{Svc.Targets.Target.DataId}");
            if (ImGui.Button("Copy DataID to clipboard"))
            {
                ImGui.SetClipboardText($"{Svc.Targets.Target.DataId}");
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

    public void ListGrabber()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();
        var PlaceNameSheet = Svc.Data.GetExcelSheet<PlaceName>();
        Dictionary<uint, string> placeNameIssuedDict = new Dictionary<uint, string>();
        foreach (var row in sheet)
        {
            if (sheet != null && row.Name.ToString() != null)
            {
                uint placeIssuedId = row.PlaceNameIssued.Value.RowId;
                string placeIssuedName = PlaceNameSheet.GetRow(placeIssuedId).Name.ToString();
                if (!placeNameIssuedDict.ContainsKey(placeIssuedId))
                {
                    placeNameIssuedDict.Add(placeIssuedId, placeIssuedName);
                }
            }
        }
        foreach (var kdp in placeNameIssuedDict)
        {
            string clipboardText = $"{{ {kdp.Key}, new ZoneInfo {{ IssuedLocation = \"{kdp.Value}\", }} }},";
            if (ImGui.Button($"Copy Info ###{kdp.Value}"))
            {
                ImGui.SetClipboardText(clipboardText);
            }
            ImGui.SameLine();
            ImGui.Text($"{$"Place: {kdp.Value}, Key: {kdp.Key}"}");
        }
    }

    public void PlaceName()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();
        List<uint> leveClient = new List<uint>();
        foreach (var row in sheet)
        {
            var jobID = row.LeveAssignmentType.Value.RowId;
            var client = row.LeveClient.Value.RowId;
            var levelreq = row.ClassJobLevel.ToInt();
            var minlv = 90;
            var maxlv = 98;
            if (CrafterJobs.Contains(jobID) && !leveClient.Contains(client) && (levelreq >= minlv && levelreq <= maxlv))
                leveClient.Add(client);
        }
        if (ImGui.Button("Copy following list"))
        {
            string clipboardText = string.Join(", ", leveClient);
            ImGui.SetClipboardText($"{clipboardText}");
        }
        foreach (var item in leveClient)
        {
            ImGui.Text($"{item}");
        }
    }
    public void ExportDict()
    {
        string exportPath = @"D:\LeveTypeDictHardcoded.cs"; // Exporting to D: root

        using StreamWriter writer = new(exportPath);
        writer.WriteLine("public static Dictionary<uint, CrafterDataDict> LeveTypeDict = new()");
        writer.WriteLine("{");

        foreach (var entry in CrafterLeves) // Directly access the dictionary
        {
            writer.WriteLine($"    {{ {entry.Key}, new CrafterDataDict {{");
            writer.WriteLine($"        Amount = {entry.Value.Amount},");
            writer.WriteLine($"        LeveName = \"{entry.Value.LeveName}\",");
            writer.WriteLine($"        JobAssignmentType = {entry.Value.JobAssignmentType},");
            writer.WriteLine($"        Level = {entry.Value.Level},");
            writer.WriteLine($"        QuestID = {entry.Value.QuestID},");
            writer.WriteLine($"        ExpReward = {entry.Value.ExpReward},");
            writer.WriteLine($"        GilReward = {entry.Value.GilReward},");
            writer.WriteLine($"        LeveVendorID = {entry.Value.LeveVendorID}");
            writer.WriteLine($"        LeveTurninVendorID = 0");
            writer.WriteLine($"        ItemID = {entry.Value.ItemID},");
            writer.WriteLine($"        ItemName = \"{entry.Value.ItemName}\",");
            writer.WriteLine($"        RepeatAmount = {entry.Value.RepeatAmount},");
            writer.WriteLine($"        TurninAmount = {entry.Value.TurninAmount},");
            writer.WriteLine($"        CurrentItemAmount = {entry.Value.CurrentItemAmount}");
            writer.WriteLine("    } },");
        }

        writer.WriteLine("};");
        Console.WriteLine($"C# dictionary exported to {exportPath}");
    }
}
