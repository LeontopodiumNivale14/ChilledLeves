using ChilledLeves.Scheduler;
using ChilledLeves.Scheduler.Tasks;
using ChilledLeves.Utilities;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using static ECommons.UIHelpers.AddonMasterImplementations.AddonMaster;

namespace ChilledLeves.Ui;

internal class DebugWindow : Window
{
    public DebugWindow() :
        base($"Chilled Leves Debug {P.GetType().Assembly.GetName().Version} ###ChilledLevesDebug") //
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
                ("Star Test", StarTester, null, true),
                ("Fish Peeps", FisherTurnin, null, true),
                ("Leve Vendor Info", LeveVendor, null, true),
                ("Gathering Debug", GatheringTest, null, true),
                ("Crafting Debug", CraftingDebug, null, true)
        );
    }

    #region Main Debug

    private int ClassID = 1;

    private void MainDebug()
    {
        ImGui.Text($"CID: {SoundAlert.CID:X16}");
        if (ImGui.Button("Copy CID"))
        {
            ImGui.SetClipboardText($"{SoundAlert.CID:X16}");
        }

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


        ImGui.Text($"Miner: {C.ShowMiner}");
        ImGui.Text($"Botanist: {C.ShowBotanist}"); //
        ImGui.Text($"Fisher: {C.ShowFisher}");
        ImGui.Text($"Carpenter: {C.ShowCarpenter}");
        ImGui.Text($"Blacksmith: {C.ShowBlacksmith}");
        ImGui.Text($"Armorer: {C.ShowArmorer}");
        ImGui.Text($"Goldsmith: {C.ShowGoldsmith}");
        ImGui.Text($"Leatherworker {C.ShowLeatherworker}");
        ImGui.Text($"Weaver: {C.ShowWeaver}");
        ImGui.Text($"Alchemist: {C.ShowAlchemist}");
        ImGui.Text($"Culinarian: {C.ShowCulinarian}");


        foreach (var entry in C.workList)
        {
            uint leve = entry.LeveID;
            ImGui.PushID((int)leve);
            string leveName = LeveDictionary[leve].LeveName;


            ImGui.SetNextItemWidth(75);
            int inputValue = entry.InputValue;
            ImGui.InputInt($"###LeveID Input {leve}", ref inputValue);
            entry.InputValue = inputValue;
            ImGui.SameLine();
            ImGui.Text($"Leve: {leveName} | Dict Value: {entry.InputValue}");
        }

        ImGui.Text($"{CurrentMap().ToString()}");

        ImGui.Text($"Pandora's Box: Auto-Select Turnin: {P.pandora.GetFeatureEnabled("Auto-select Turn-ins")}");
        ImGui.SameLine();
        if (ImGui.Button("Toggle Auto-Select Turn-ins"))
        {
            P.pandora.PauseFeature("Auto-select Turn-ins", 1000);
        }
        if (ImGui.Button("Toggle Auto-Select Turn-ins Config"))
        {
            P.pandora.SetFeatureEnabled("Auto-select Turn-ins", false);
        }
        ImGui.Text($"Pandora's Box: Auto-Select Turnin: {P.pandora.GetConfigEnabled("Auto-select Turn-ins", "AutoConfirm")}");
    }

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
            ImGui.SetClipboardText($"{Player.Position.X}f, {Player.Position.Y}f, {Player.Position.Z}f,");
        }
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{Player.Position}");

        if (Svc.Targets?.Target != null)
        {
            // Get the GameObjectId and display it in the ImGui.Text box
            ImGui.Text($"Name: {Svc.Targets.Target.Name}");
            ImGui.Text($"{Svc.Targets.Target.DataId}");
            if (ImGui.Button("Copy DataID to clipboard"))
            {
                ImGui.SetClipboardText($"{Svc.Targets.Target.DataId}");
            }
            ImGui.Text($"Target Pos: {Svc.Targets.Target.Position}");
            if (ImGui.Button("Copy Target XYZ"))
            {
                Vector3 roundedVector = new Vector3(
                    MathF.Round(Svc.Targets.Target.Position.X, 2),
                    MathF.Round(Svc.Targets.Target.Position.Y, 2),
                    MathF.Round(Svc.Targets.Target.Position.Z, 2)
                    );
                ImGui.SetClipboardText($"{roundedVector.X}f, {roundedVector.Y}f, {roundedVector.Z}f");
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

            ImGui.SameLine();
            if (ImGui.Button("Copy Target's X & Z"))
                ImGui.SetClipboardText($"{Svc.Targets.Target.Position.X}f, {Svc.Targets.Target.Position.Z}f");
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
                        var NPCLocation = entry.Value.NPCInteractZone;
                        var NPCId = entry.Key;

                        if (IsInZone(zoneID) && GetDistanceToPlayer(NPCLocation) > 0.5f)
                        {
                            SchedulerMain.HandleMountAndMove(NPCLocation, NPCId);
                        }
                        else
                        {
                            PluginVerbos("Not valid for movement");
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

    private static int col1Width = 0;
    private static int col2Width = 0;
    private static int col3Width = 0;

    private static string turninNPCResult = "";
    private static string LeveNPCResult = "";
    private static string LeveNameSearch = "";

    private static int gilAmount = 0;
    private static int minLevel = 0;

    private static HashSet<uint> visibleLeveIds = new();

    #nullable disable
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

        ImGui.SetNextItemWidth(125);
        ImGui.InputText("###LeveNameSearch", ref LeveNameSearch, 200);
        if (string.IsNullOrEmpty(LeveNameSearch))
        {
            var cursorPos = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new System.Numerics.Vector2(cursorPos.X + 5, cursorPos.Y - ImGui.GetTextLineHeightWithSpacing()));
            ImGui.TextDisabled("Leve Name Search");
            ImGui.SetCursorPos(cursorPos); // Reset cursor to avoid overlap issues
        }
        LeveNameSearch = LeveNameSearch.Trim();

        ImGui.SetNextItemWidth(125);
        ImGui.InputInt("Gil Input", ref gilAmount);

        ImGui.SetNextItemWidth(300);
        ImGui.SliderInt("Min Level", ref minLevel, 1, 98);

        ImGui.SameLine();
        if (ImGui.Button("Copy all leveIds"))
        {
            // You now have a HashSet of visible leve IDs
            // You can store, print, or export them as needed
            var copy = new HashSet<uint>(visibleLeveIds);

            // Example: Print to console
            foreach (var id in copy)
                PluginLog.Log($"Visible Leve ID: {id}");

            // Or copy to clipboard as CSV
            ImGui.SetClipboardText(string.Join(", ", copy));
            visibleLeveIds.Clear();
        }

        if (ImGui.BeginTable("NPC Info Table", 14, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn("Amount", ImGuiTableColumnFlags.WidthFixed, col1Width);
            ImGui.TableSetupColumn("LeveID", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Leve Name", ImGuiTableColumnFlags.WidthFixed, col2Width);
            ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed, col3Width);
            ImGui.TableSetupColumn("JobType", ImGuiTableColumnFlags.WidthFixed, 50);
            ImGui.TableSetupColumn("Leve Vendor Name");
            ImGui.TableSetupColumn("Leve Turnin Vendor Name");
            ImGui.TableSetupColumn("Zone Start");
            ImGui.TableSetupColumn("Zone End");
            ImGui.TableSetupColumn("Leve Cost");
            ImGui.TableSetupColumn("EXP Reward");
            ImGui.TableSetupColumn("Gil Reward");
            ImGui.TableSetupColumn("Item Name");
            ImGui.TableSetupColumn("Turnin Amount");
            ImGui.TableSetupColumn("Current Amount");

            ImGui.TableHeadersRow();

            foreach (var entry in LeveDictionary)
            {
                string itemAmountText = entry.Value.Amount.ToString();
                uint LeveId = entry.Key;
                string leveName = entry.Value.LeveName.ToString();
                uint leveLevel = entry.Value.Level;
                uint JobTypeNumber = entry.Value.JobAssignmentType;
                string leveVendorName = entry.Value.LeveVendorName.ToString();
                string leveVendorId = entry.Value.LeveVendorID.ToString();
                string leveStartName = ZoneName(LeveNPCDict[entry.Value.LeveVendorID].ZoneID);
                int leveCost = entry.Value.AllowanceCost;
                int gilReward = entry.Value.GilReward;
               
                string turninVendorName = "Not a valid turnin vendor!";
                uint turninVendorId = 0;
                string leveEndZoneName = "";

                if (CraftFisherJobs.Contains(JobTypeNumber))
                {
                    turninVendorId = CraftDictionary[entry.Key].LeveTurninVendorID;
                    if (turninVendorId != 0)
                    {
                        turninVendorName = LeveNPCDict[turninVendorId].Name;
                        leveEndZoneName = ZoneName(LeveNPCDict[turninVendorId].ZoneID);
                    }
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
                if (!string.IsNullOrEmpty(LeveNameSearch))
                {
                    if (!leveName.Contains(LeveNameSearch, StringComparison.OrdinalIgnoreCase))
                        continue;
                }
                if (gilAmount > gilReward)
                {
                    continue;
                }
                if (minLevel > leveLevel)
                {
                    continue;
                }

                if (!visibleLeveIds.Contains(LeveId))
                {
                    visibleLeveIds.Add(LeveId);
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

                // Leve ID (Row 1)
                ImGui.TableNextColumn();
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{LeveId}");


                // Leve Name/Class (Row 2)
                ImGui.TableNextColumn();

                ImGui.Image(LeveTypeDict[entry.Value.JobAssignmentType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
                ImGui.SameLine(0, 5);
                ImGui.AlignTextToFramePadding();
                ImGui.Text($"{leveName}");
                col2Width = Math.Max(itemAmountText.Length + 30, col2Width);

                // Level of the Leve (Row 3)
                ImGui.TableNextColumn();
                ImGui.Text($"{leveLevel}");

                // Job Type 
                ImGui.TableNextColumn();
                ImGui.Text($"{JobTypeNumber}");

                // Leve Vendor Name/ID (Row 4)
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{leveVendorName}"))
                {
                    var flagX = LeveNPCDict[entry.Value.LeveVendorID].flagX;
                    var flagZ = LeveNPCDict[entry.Value.LeveVendorID].flagZ;
                    SetFlagForNPC(LeveNPCDict[entry.Value.LeveVendorID].ZoneID, flagX, flagZ);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text($"ID: {leveVendorId}");
                    ImGui.EndTooltip();
                }

                // Leve Vendor Name/ID (Row 5)
                ImGui.TableNextColumn();
                if (CraftFisherJobs.Contains(JobTypeNumber))
                {
                    if (ImGui.Selectable($"{turninVendorName}"))
                    {
                        var flagX = LeveNPCDict[turninVendorId].flagX;
                        var flagZ = LeveNPCDict[turninVendorId].flagZ;
                        SetFlagForNPC(LeveNPCDict[turninVendorId].ZoneID, flagX, flagZ);
                    }
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text($"ID: {turninVendorId}");
                        ImGui.EndTooltip();
                    }
                }

                // Leve Start Zone (Row 6)
                ImGui.TableNextColumn();
                ImGui.Text(leveStartName);

                // Leve End Zone (Row 7)
                ImGui.TableNextColumn();
                ImGui.Text(leveEndZoneName);

                // Leve Cost (Row 8)
                ImGui.TableNextColumn();
                ImGui.Text(leveCost.ToString());

                // Leve XP Reward (Row 9)
                ImGui.TableNextColumn();

                // Gil Reward Amount (Row 10)
                ImGui.TableNextColumn();
                ImGui.Text($"{gilReward}");

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
            PluginVerbos("Success");
        }
    }

    public List<uint> FishPeeps = new List<uint>();

    public void FisherTurnin()
    {
        var sheet = Svc.Data.GetExcelSheet<Leve>();
        foreach (var leveID in sheet)
        {
            if (leveID.LeveAssignmentType.Value.RowId != 4)
            {
                continue;
            }

            var leveCliendID = leveID.LeveClient.Value.RowId;

            if (!FishPeeps.Contains(leveCliendID))
            {
                FishPeeps.Add(leveCliendID);
            }
        }

        foreach (var clientId in FishPeeps)
        {
            ImGui.Text($"ClientID: {clientId}");
        }
    }

    private static SortedDictionary<uint, string> ViableLeves = new SortedDictionary<uint, string>();

    public unsafe void LeveVendor()
    {
        if (ImGui.Button("Clear Dictionary"))
        {
            ViableLeves.Clear();
        }

        foreach (var leve in C.workList)
        {
            if (!ViableLeves.ContainsKey(leve.LeveID))
            {
                string leveName = LeveDictionary[leve.LeveID].LeveName;
                ViableLeves.Add(leve.LeveID, leveName);
            }
        }
        foreach (var leve in ViableLeves)
        {
            ImGui.Text($"Leves in Dictionary: {leve.Value}");
        }


        if (ImGui.CollapsingHeader("Debug"))
        {
            if (TryGetAddonMaster<GuildLeve>("GuildLeve", out var m) && m.IsAddonReady)
            {
                ImGui.Text("All Available Leves:");
                foreach (var l in m.Levequests)
                {
                    var leveMatch = ViableLeves.FirstOrDefault(v => v.Value == l.Name);

                    if (!leveMatch.Equals(default(KeyValuePair<uint, string>)))
                    {
                        ImGui.Text($"{leveMatch.Value} [{leveMatch.Key}]");
                    }
                }
                ImGui.Text("- - - - - ");

                foreach (var l in m.Levequests)
                {
                    ImGuiEx.Text($"{l.Name} ({l.Level})");
                    ImGui.SameLine();
                    if (ImGui.SmallButton("Select##" + l.Name)) l.Select();
                }
                ref var r = ref Ref<int>.Get("Leve");
                ImGui.InputInt("id", ref r);
                if (ImGui.Button("Callback"))
                {
                    ECommons.Automation.Callback.Fire(m.Base, true, 13, 1, r);
                }
                if (TryGetAddonMaster<AddonMaster.JournalDetail>("JournalDetail", out var det) && det.IsAddonReady)
                {
                    if (det.CanInitiate)
                    {
                        if (ImGui.Button("Initiate")) det.Initiate();
                    }
                }
            }
        }
    }

    #endregion

    #region Gathering Testing

    private static List<uint> NodeIds = new List<uint>();

    public void GatheringTest()
    {
        ImGui.Text("Statuses");
        ImGui.Text($"Gathering [Normal]: {Svc.Condition[ConditionFlag.Gathering]}");
        ImGuiEx.HelpMarker("Interacting with Gathering Node", sameLine: true);

        ImGui.Text($"Gathering [Gathering42] {Svc.Condition[ConditionFlag.Gathering42]}");
        ImGuiEx.HelpMarker("Interacting with Gathering Node/Using Buffs", sameLine: true);

        foreach (var x in Svc.Objects)
        {
            if (x.ObjectKind == ObjectKind.GatheringPoint)
            {
                Vector3 rounded = new Vector3(
                    MathF.Round(x.Position.X, 2),
                    MathF.Round(x.Position.Y, 2),
                    MathF.Round(x.Position.Z, 2)
                    );

                ImGuiEx.Text($"Gathering Point: {x.DataId} |  Location: {rounded} | Distance: {GetDistanceToPlayer(x):N2} |  Targetable: {x.IsTargetable}");
            }
        }

        if (TryGetAddonMaster<AddonMaster.Gathering>("Gathering", out var m) && m.IsAddonReady)
        {
            ImGui.Text("Gathering Test");
            ImGui.Text($"Current Integrity: {m.CurrentIntegrity}");
            ImGui.Text($"Total Integrity: {m.TotalIntegrity}");
            ImGui.Text($"Node ID: {Svc.Targets.Target.DataId}");
            ImGui.Text($"Type: {Svc.Targets.Target.ObjectKind}");

            foreach (var item in m.GatheredItems)
            {
                if (item.ItemID == 0)
                    continue;

                ImGui.Text($"{item.ItemName} ID: ({item.ItemID})");
                ImGui.Text($"Gathering Chance: {item.GatherChance} | Boon %%: {item.BoonChance}");
                ImGui.SameLine();
                if (ImGui.Button("Select##" + item.ItemName)) item.Gather();
            }
        }
    }

    #endregion

    #region Crafting Testing

    /// <summary>
    /// Need to write this out so I can logic this out in case I don't finish this in the next 10 minutes
    /// 1: Check the main craft for all required items
    /// 2: Check to see if the items are a craftable item 
    ///   -> If yes, check the recipe sheet for all required items
    ///     -> Check all the items in that item to see if it's a craftable item
    ///     -> Rince and repeat until I get down to the base items
    ///     -> For each pre-craft it hits, make sure to add it to the "All Items List"
    ///     -> As well as the base items as well to the "All Items List"
    ///   -> If no, add it to the "All Items List"
    /// 3: Once I have all the items, run a loop to check all the items, and see which ones are craftable
    /// 4: Add each craftable once to the "Artisan Pre Crafts" list
    /// </summary>

    private static void CraftingDebug()
    {
        // quick references for the sheets that I'm using
        var LeveSheet = Svc.Data.GetExcelSheet<Leve>();
        var RecipeSheet = Svc.Data.GetExcelSheet<Recipe>();

        if (ImGui.Button("Update Item List"))
        {
            AllItems.Clear();
            ArtisanPreCrafts.Clear();
            ArtisanFinalCrafts.Clear();

            foreach (var LeveEntry in C.workList)
            {
                var LeveId = LeveEntry.LeveID;
                if (!CraftDictionary.ContainsKey(LeveId))
                    continue;

                var ItemId = CraftDictionary[LeveId].ItemID;
                CheckItems(ItemId, LeveEntry.InputValue, RecipeSheet);

                var RecipeRow = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == ItemId);

                if (ArtisanFinalCrafts.ContainsKey(ItemId))
                    ArtisanFinalCrafts[ItemId] += LeveEntry.InputValue * RecipeRow.AmountResult.ToInt();
                else
                    ArtisanFinalCrafts.Add(ItemId, (LeveEntry.InputValue * RecipeRow.AmountResult.ToInt()));
            }

            // Now break down all ingredients recursively
            foreach (var kvp in AllItems)
            {
                var itemSearch = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == kvp.Key);
                if (itemSearch.RowId != 0)
                {
                    var itemId = kvp.Key;
                    var amount = kvp.Value;

                    if (!ArtisanPreCrafts.ContainsKey(itemId))
                        ArtisanPreCrafts.Add(itemId, amount);
                }
            }
        }

        ImGui.Text("- - - All Items - - - ");
        foreach (var item in AllItems)
        {
            var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

            ImGui.Text($"Item ID [{item.Key}]: {itemName} | Amount: {item.Value}");
        }

        ImGui.Text("- - - Artisan Pre Crafts - - -");
        foreach (var item in ArtisanPreCrafts)
        {
            var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

            ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
        }

        ImGui.Spacing();

        ImGui.Text("- - - Artisan Final Crafts - - -");
        foreach (var item in ArtisanFinalCrafts)
        {
            var itemName = Svc.Data.GetExcelSheet<Item>().GetRow(item.Key).Name.ToString();

            ImGui.Text($"Item ID: {itemName} | Amount: {item.Value}");
        }
    }

    private static void CheckItems(uint itemId, int runAmount, ExcelSheet<Recipe> RecipeSheet)
    {
        var recipe = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == itemId);
        if (recipe.RowId == 0)
            return; // Not a craftable item; likely a raw mat

        for (int i = 0; i < 6; i++)
        {
            PluginDebug($"Ingrediant[{i}]");

            // Checks to see if the ingrediant is valid
            var ingrediant = recipe.Ingredient[i].Value.RowId;
            if (ingrediant == 0)
                continue;

            var amount = recipe.AmountIngredient[i].ToInt() * runAmount;

            if (AllItems.ContainsKey(ingrediant))
            {
                PluginDebug($"Ingrediant: {ingrediant} exist currently, updating amount");
                AllItems[ingrediant] += amount;
            }
            else if (!AllItems.ContainsKey(ingrediant))
            {
                PluginDebug($"Ingrediant: {ingrediant} does not exist, adding to list");
                AllItems.Add(ingrediant, amount);
            }

            var subRecipe = RecipeSheet.FirstOrDefault(r => r.ItemResult.Value.RowId == ingrediant);
            if (subRecipe.RowId != 0)
            {
                // If the item is a recipe, check the recipe sheet for all ingredients
                CheckItems(ingrediant, runAmount, RecipeSheet);
            }
        }

    }

    #endregion
}