using ChilledLeves.Scheduler.Tasks;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using System.Globalization;

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
                ImGui.SetClipboardText($"{Svc.Targets.Target.Position.X}f, {Svc.Targets.Target.Position.Y}f, {Svc.Targets.Target.Position.Z}f, ");
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
}
