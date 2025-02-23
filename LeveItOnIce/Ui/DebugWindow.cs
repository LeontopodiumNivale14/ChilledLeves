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
using System.Runtime.Intrinsics.Arm;
using ECommons.ExcelServices;

namespace ChilledLeves.Ui;

internal class DebugWindow : Window
{
    public DebugWindow() :
        base($"Chilled Leves Debug {P.GetType().Assembly.GetName().Version} ###ChilledLevesDebug")
    {
        Flags = ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(100, 100),
            MaximumSize = new Vector2(3000, 3000)
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
                ("Leve Table Debug ###LeveItAloneTable", LeveItAloneTable, null, true),
                ("NPC Vendor's", TeleportTest, null, true),
                ("Quest Checker", QuestChecker, null, true),
                ("Star Test", StarTester, null, true)
        );

    }

    private int LeveID = 0;
    private int ClassID = 1;

    private void MainDebug()
    {
        ImGui.SetNextItemWidth(75);
        if (ImGui.InputInt("###ClassIDDebug", ref ClassID))
        {
            if (ClassID < 1)
                ClassID = 1;
            else if (ClassID > 42)
                ClassID = 42;
        }

        if (ImGui.Button("Class Change"))
        {
            TaskClassChange.Enqueue((Job)ClassID);
        }
        ImGui.Text($"Job Exp: {GetJobExp((uint)ClassID)}");


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
            ImGui.Text($"{GetDistanceToPlayer(Svc.Targets.Target)}");
        }
        else
        {
            // Optionally display a message if no target is selected
            ImGui.Text("No target selected.");
        }
    }

    string ZoneSearch = "";

    public void TeleportTest()
    {
        if (ImGui.Button("Mounting Test"))
        {
            TaskMountUp.Enqueue();
        }

        ImGui.SameLine(0, 5);

        if (ImGui.Button("Dismount"))
        {
            TaskDisMount.Enqueue();
        }

        if (Svc.Targets?.Target != null)
        {
            TargetXPos = (float)Math.Round(Svc.Targets.Target.Position.X, 2);
            TargetZPos = (float)Math.Round(Svc.Targets.Target.Position.Z, 2);

            ImGui.SameLine();
            if (ImGui.Button("Copy Target's X & Z"))
                ImGui.SetClipboardText($"flagX = {TargetXPos.ToString("0.00", CultureInfo.InvariantCulture)}f, " +
                                       $"flagZ = {TargetZPos.ToString("0.00", CultureInfo.InvariantCulture)}f");
        }

        ImGui.SetNextItemWidth(150);
        ImGui.InputText("###Search for Zone", ref ZoneSearch, 200);

        if (string.IsNullOrEmpty(ZoneSearch))
        {
            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new System.Numerics.Vector2(cursorPos.X + 5, cursorPos.Y - ImGui.GetTextLineHeightWithSpacing()));
            ImGui.TextDisabled("Zone Searching");
            ImGui.SetCursorPos(cursorPos); // Reset cursor to avoid overlap issues
        }
        ZoneSearch = ZoneSearch.Trim();

        if (ImGui.BeginChild("Debug Leve Peeps V2"))
        {
            if (ImGui.BeginTable("Debug Leve Table V2", 4, ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("NPC Name", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Location", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Teleport", ImGuiTableColumnFlags.WidthFixed, 200);
                ImGui.TableSetupColumn("Flag", ImGuiTableColumnFlags.WidthFixed, 75);

                ImGui.TableHeadersRow();

                foreach (var entry in LeveNPCDict)
                {
                    string zoneName = ZoneName(entry.Value.ZoneID);

                    if (!string.IsNullOrEmpty(zoneName))
                    {
                        if (!zoneName.Contains(ZoneSearch, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    ImGui.TableNextRow();

                    var aetheryte = entry.Value.Aetheryte;
                    var zoneID = entry.Value.ZoneID;

                    ImGui.TableSetColumnIndex(0);

                    ImGui.PushID((int)entry.Key);
                    if (ImGui.Button($"{entry.Value.Name}"))
                    {
                        if (!IsInZone(zoneID))
                        {
                            TaskTeleport.Enqueue(aetheryte, zoneID);
                        }
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"NPC ID: {entry.Key}");
                        ImGui.EndTooltip();
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text(zoneName);
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"Zone ID: {entry.Value.ZoneID}");
                        ImGui.EndTooltip();
                    }

                    ImGui.TableNextColumn();
                    if (ImGui.Button("Move to NPC"))
                    {
                        var NPCLocation = entry.Value.NPCLocation;
                        bool mount = false;
                        bool fly = false;
                        if (entry.Value.Mount)
                        {
                            mount = entry.Value.Mount;
                        }
                        if (entry.Value.Fly.HasValue)
                        {
                            fly = entry.Value.Fly.Value;
                        }

                        if (IsInZone(zoneID) && GetDistanceToPlayer(NPCLocation) > 0.5f)
                        {
                            if (mount)
                            {
                                P.taskManager.Enqueue(() => PluginLog($"{zoneID} is on the mounting list of enabled"));
                                TaskMountUp.Enqueue();
                            }
                            if (fly)
                            {
                                P.taskManager.Enqueue(() => PluginLog($"{zoneID} is on the fly zone. GO GO GO."));
                                fly = true;
                            }

                            P.taskManager.Enqueue(() => PluginLog($"Flying: {fly}, moving to: {NPCLocation}"));
                            TaskMoveTo.Enqueue(NPCLocation, "LeveNPC", fly, 0.5f);
                            TaskDisMount.Enqueue();

                        }
                        else
                        {
                            PluginLog("Not valid for movement");
                        }
                    }

                    ImGui.TableNextColumn();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Flag, "Flag Button"))
                    {
                        var flagX = entry.Value.flagX;
                        var flagZ = entry.Value.flagZ;
                        SetFlagForNPC(entry.Value.ZoneID, flagX, flagZ);
                    }

                    ImGui.PopID();
                }
            }
            ImGui.EndTable();
        }
        ImGui.EndChild();
    }

    #pragma warning disable CS8602 // Dereference of a possibly null reference.

    private static int col1Width = 0;
    private static int col2Width = 0;
    private static int col3Width = 0;

    private static string turninNPCResult = "";
    private static string LeveNPCResult = "";

    public void LeveItAloneTable()
    {
        ImGui.SetNextItemWidth(125);
        ImGui.InputText("###turninNPC", ref turninNPCResult, 200);
        // If the input is empty, draw placeholder text
        if (string.IsNullOrEmpty(turninNPCResult))
        {
            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new System.Numerics.Vector2(cursorPos.X + 5, cursorPos.Y - ImGui.GetTextLineHeightWithSpacing()));
            ImGui.TextDisabled("Turnin NPC Search");
            ImGui.SetCursorPos(cursorPos); // Reset cursor to avoid overlap issues
        }
        turninNPCResult = turninNPCResult.Trim();

        ImGui.SetNextItemWidth(125);
        ImGui.InputText("###leveNPCsearch", ref LeveNPCResult, 200);
        if (string.IsNullOrEmpty(LeveNPCResult))
        {
            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new System.Numerics.Vector2(cursorPos.X + 5, cursorPos.Y - ImGui.GetTextLineHeightWithSpacing()));
            ImGui.TextDisabled("Leve NPC Search");
            ImGui.SetCursorPos(cursorPos); // Reset cursor to avoid overlap issues
        }
        LeveNPCResult = LeveNPCResult.Trim();


        if (ImGui.BeginTable("NPC Info Table", 12, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, col1Width);
            ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, col2Width);
            ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, col3Width);
            ImGui.TableSetupColumn("Leve Vendor Name");
            ImGui.TableSetupColumn("Leve Turnin Vendor Name");
            ImGui.TableSetupColumn("Zone Start");
            ImGui.TableSetupColumn("Zone End");
            ImGui.TableSetupColumn("QuestID");
            ImGui.TableSetupColumn("EXP Reward");
            ImGui.TableSetupColumn("Gil Reward");
            ImGui.TableSetupColumn("Item Name");
            ImGui.TableSetupColumn("Turnin Amount");
            ImGui.TableSetupColumn("Current Amount");

            ImGui.TableHeadersRow();

            foreach (var entry in CrafterLeves)
            {
                string itemAmountText = entry.Value.Amount.ToString();
                string leveName = entry.Value.LeveName.ToString();
                string leveLevel = entry.Value.Level.ToString();
                string leveVendorName = entry.Value.LeveVendorName.ToString();
                string leveVendorId = entry.Value.LeveVendorID.ToString();
                string leveStartName = ZoneName(LeveNPCDict[entry.Value.LeveVendorID].ZoneID);
                uint turninVendorId = entry.Value.LeveTurninVendorID;
                string leveEndZoneName = ZoneName(LeveNPCDict[turninVendorId].ZoneID);
                string turninVendorName = "";
                if (turninVendorId != 0)
                {
                    turninVendorName = LeveNPCDict[turninVendorId].Name;
                }
                else
                {
                    turninVendorName = "Not a valid turnin vendor!";
                }

                // Search Filtering
                if (!string.IsNullOrEmpty(turninNPCResult))
                {
                    if (!turninVendorName.Contains(turninNPCResult, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                if (!string.IsNullOrEmpty(LeveNPCResult))
                {
                    if (!leveVendorName.Contains(LeveNPCResult, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                ImGui.TableNextRow();

                ImGui.PushID((int)entry.Key);

                // Item Amount (Row 0)
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"{itemAmountText}");
                col1Width = Math.Max(itemAmountText.Length, col1Width);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("Amount that is currently set to run");
                    ImGui.EndTooltip();
                }

                // Leve Name/Class (Row 1)
                ImGui.TableNextColumn();
                
                ImGui.Image(LeveTypeDict[entry.Value.JobAssignmentType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
                ImGui.SameLine(0, 5);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{leveName}");
                col2Width = Math.Max(itemAmountText.Length + 30, col2Width);

                // Level of the Leve (Row 2)
                ImGui.TableNextColumn();
                ImGui.Text($"{leveLevel}");

                // Leve Vendor Name/ID (Row 3)
                ImGui.TableNextColumn();
                ImGui.Text($"{leveVendorName}");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"ID: {leveVendorId}");
                    ImGui.EndTooltip();
                }

                // Leve Vendor Name/ID (Row 4)
                ImGui.TableNextColumn();
                ImGui.Text($"{turninVendorName}");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"ID: {turninVendorId}");
                    ImGui.EndTooltip();
                }

                // Leve Start Zone (Row 5)
                ImGui.TableNextColumn();
                ImGui.Text(leveStartName);

                // Leve End Zone (Row 6)
                ImGui.TableNextColumn();
                ImGui.Text(leveEndZoneName);

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }

    public void QuestChecker()
    {
        bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
        bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
        bool GridaniaNPC = IsMSQComplete(65665); // Spirithold Broken

        ImGui.Text("Started in:");
        ImGui.Text($"Limsa?    -> {LimsaLeveNPC}");
        ImGui.Text($"Ul' Dah? -> {UlDahLeveNPC}");
        ImGui.Text($"Gridania -> {GridaniaNPC}");

        // The 3 possible starting leves:
        bool levesofBentbranch = IsMSQComplete(65756); // Gridania startpoint
        bool levesofHorizon = IsMSQComplete(66223); // Ul'Dah startpoint
        bool levesofSwiftperch = IsMSQComplete(66229); // Limsa startpoint

        bool BothQuest()
        {
            bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
            bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
            bool GridaniaNPC = IsMSQComplete(65665); // Spirithold Broken

            if (LimsaLeveNPC)
                return levesofSwiftperch;
            else if (UlDahLeveNPC)
                return levesofHorizon;
            else if (GridaniaNPC)
                return levesofBentbranch;
            else
                return false;
        }

        ImGui.Text($"Able to pick up leves from other locations: {BothQuest()}");

        ImGui.NewLine();

        // Quest ID's attached to the leve vendor (necessary for these to be done for turnins/pickups)
        uint LevesofWineport = 65550;
        uint LevesofCampBluefog = 65551;
        uint LevesoftheObservatorium = 65552;
        uint LevesofWhitbrim = 65553;
        uint LevesofSaintCoinachsFind = 65554;
        uint LevesofHawthorne = 65757;
        uint LevesofQuarrymill = 65979;
        uint LevesofCampTranquil = 65980;
        uint LevesofCampDrybone = 66224;
        uint LevesofLittleAlaMhigo = 66228;
        uint LevesofAleport = 66230;
        uint LevesofMoraby = 66231;
        uint LevesofCostadelSol = 66232;

        ImGui.Text($"Wineport Unlocked: {CanDoLeves(LevesofWineport)}");
        ImGui.Text($"Camp Bluefog Unlocked: {CanDoLeves(LevesofCampBluefog)}");
        ImGui.Text($"The Observatorium Unlocked: {CanDoLeves(LevesoftheObservatorium)}");
        ImGui.Text($"Whitbrim Unlocked: {CanDoLeves(LevesofWhitbrim)}");
        ImGui.Text($"Staint Coinachs Find Unlocked: {CanDoLeves(LevesofSaintCoinachsFind)}");
        ImGui.Text($"Hawthorne Unlocked: {CanDoLeves(LevesofHawthorne)}");
        ImGui.Text($"Quarrymill Unlocked: {CanDoLeves(LevesofQuarrymill)}");
        ImGui.Text($"Camp Tranquil Unlocked: {CanDoLeves(LevesofCampTranquil)}");
        ImGui.Text($"Camp Drybone Unlocked: {CanDoLeves(LevesofCampDrybone)}");
        ImGui.Text($"Little Ala Mhigo Unlocked: {CanDoLeves(LevesofLittleAlaMhigo)}");
        ImGui.Text($"Aleport Unlocked: {CanDoLeves(LevesofAleport)}");
        ImGui.Text($"Moraby Unlocked: {CanDoLeves(LevesofMoraby)}");
        ImGui.Text($"Costa del Sol Unlocked: {CanDoLeves(LevesofCostadelSol)}");

        bool CanDoLeves(uint QuestRequired)
        {
            return (BothQuest() && IsMSQComplete(QuestRequired));
        }

    }

    public void StarTester()
    {
        var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
        if (ImGui.ImageButton(starTex.ImGuiHandle, new Vector2(20), new Vector2(0.0000f, 0.0000f), new Vector2(0.3333f, 0.5000f)))
        {
            PluginLog("Success");
        }
    }

    #pragma warning restore CS8602 // Dereference of a possibly null reference.
}
