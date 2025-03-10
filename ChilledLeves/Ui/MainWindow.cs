using ChilledLeves.Scheduler;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Style;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lumina.Data.Parsing.Uld.UldRoot;

namespace ChilledLeves.Ui;

internal class MainWindow : Window
{
    private static int selectedLeve;

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

        PopulateDictionary();
    }

    public void Dispose() { }

    public override void Draw()
    {
        using (ImRaii.Disabled(SchedulerMain.AreWeTicking))
        {
            if (ImGui.Button("Start"))
            {
                SchedulerMain.WorkListMode = true;
                SchedulerMain.EnablePlugin();
            }
        }
        using (ImRaii.Disabled(!SchedulerMain.AreWeTicking))
        {
            ImGui.SameLine();
            if (ImGui.Button("Stop"))
            {
                SchedulerMain.DisablePlugin();
            }
        }
        ImGui.BeginGroup();
        DrawFilterPanel();
        ImGui.EndGroup();

        ImGui.SameLine();

        ImGui.BeginGroup();
        ImGui.Dummy(new Vector2(20));
        ImGui.EndGroup();

        ImGui.SameLine();

        ImGui.BeginGroup();
        ImGui.Dummy(new Vector2(0, 26));
        ImGui.Text($"Allowances: {Allowances}/100");
        ImGui.Text($"Next 3 in: {NextAllowances:hh':'mm':'ss}");
        ImGui.Spacing();
        if (ImGui.Button("Open Worklist"))
        {
            P.workListUi.IsOpen = !P.workListUi.IsOpen;
        }
        ImGui.SameLine();
        if (ImGui.Button("Open Gathering Grind"))
        {
            P.gatherModeUi.IsOpen = !P.gatherModeUi.IsOpen;
        }
        ImGui.EndGroup();
        ImGui.Separator();

        DrawList();
    }

    private void DrawFilterPanel()
    {
        ImGui.Text("Filter Leves");

        DrawButtonStar("Show only favorites", ref C.OnlyFavorites, true);
        DrawLeveStatusButton();
        DummyButton(7);
        DrawButton(5, $"Leves", ref C.ShowCarpenter, true);
        DrawButton(6, $"Leves", ref C.ShowBlacksmith, true);
        DrawButton(7, $"Leves", ref C.ShowArmorer, true);
        DrawButton(8, $"Leves", ref C.ShowGoldsmith, true);
        DrawButton(9, $"Leves", ref C.ShowLeatherworker, true);
        DrawButton(10, $"Leves", ref C.ShowWeaver, true);
        DrawButton(11, $"Leves", ref C.ShowAlchemist, true);
        DrawButton(12, $"Leves", ref C.ShowCulinarian, true);
        DummyButton(1);
        ImGui.SameLine();
        DrawButton(4, $"Leves", ref C.ShowFisher, false);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Level:");
        ImGui.SameLine();
        var level = C.LevelFilter > 0 ? C.LevelFilter.ToString() : "";
        ImGui.SetNextItemWidth(30);
        if (ImGui.InputText("###Level", ref level, 3))
        {
            C.LevelFilter = (int)(uint.TryParse(level, out var num) && num > 0 ? num : 0);
            C.Save();
        }
        ImGui.SameLine();
        ImGui.Text("Name:");
        ImGui.AlignTextToFramePadding();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(220);
        if (ImGui.InputText("###Name", ref C.NameFilter, 200))
        {
            C.NameFilter = C.NameFilter.Trim();
            C.Save();
        }
    }

    /// <summary>
    /// Spacer for the buttons, that way I don't have to re-write the same thing 20x times
    /// </summary>
    /// <param name="repeatamount"></param>
    private static void DummyButton(int repeatamount)
    {
        for (int i = 0; i < repeatamount; i++)
        {
            ImGui.Dummy(new Vector2(24));
            if (i < repeatamount - 1)
                ImGui.SameLine();
        }
    }

    #nullable disable
    private void DrawButton(uint row, string tooltip, ref bool state, bool sameLine)
    {
        ISharedImmediateTexture? icon = null;

        if (state)
        {
            icon = LeveTypeDict[row].AssignmentIcon;
        }
        else if (!state)
        {
            icon = GreyTexture[row];
        }

        string tooltipText = $"{LeveTypeDict[row].LeveClassType} {tooltip}";

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        float scaleFactor = 1.0f;
        if (!state)
        {
            scaleFactor = 1.4f;
        }
        Vector2 baseSize = new Vector2(24, 24);
        Vector2 uv0 = new Vector2(0.5f - 0.5f / scaleFactor, 0.5f - 0.5f / scaleFactor);
        Vector2 uv1 = new Vector2(0.5f + 0.5f / scaleFactor, 0.5f + 0.5f / scaleFactor);
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, baseSize, uv0, uv1))
        {
            state = !state;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltipText);
            ImGui.Text($"Showing Leves: {state}");
            ImGui.EndTooltip();
        }
        if (sameLine)
            ImGui.SameLine();
    }

    #region Specific Buttons
    private void CRPButton(string tooltip, bool sameLine)
    {
        ISharedImmediateTexture? icon = null;
        bool state = C.ShowCarpenter;
        uint row = 5;

        if (state)
        {
            icon = LeveTypeDict[row].AssignmentIcon;
        }
        else if (!state)
        {
            icon = GreyTexture[row];
        }

        string tooltipText = $"{LeveTypeDict[row].LeveClassType} {tooltip}";

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        float scaleFactor = 1.0f;
        if (!state)
        {
            scaleFactor = 1.4f;
        }
        Vector2 baseSize = new Vector2(24, 24);
        Vector2 uv0 = new Vector2(0.5f - 0.5f / scaleFactor, 0.5f - 0.5f / scaleFactor);
        Vector2 uv1 = new Vector2(0.5f + 0.5f / scaleFactor, 0.5f + 0.5f / scaleFactor);
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, baseSize, uv0, uv1))
        {
            C.ShowCarpenter = !C.ShowCarpenter;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltipText);
            ImGui.Text($"Showing Leves: {state}");
            ImGui.EndTooltip();
        }
        if (sameLine)
            ImGui.SameLine();
    }
    private void BSMButton(string tooltip, bool sameLine)
    {
        ISharedImmediateTexture? icon = null;
        bool state = C.ShowBlacksmith;
        uint row = 6;

        if (state)
        {
            icon = LeveTypeDict[row].AssignmentIcon;
        }
        else if (!state)
        {
            icon = GreyTexture[row];
        }

        string tooltipText = $"{LeveTypeDict[row].LeveClassType} {tooltip}";

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        float scaleFactor = 1.0f;
        if (!state)
        {
            scaleFactor = 1.4f;
        }
        Vector2 baseSize = new Vector2(24, 24);
        Vector2 uv0 = new Vector2(0.5f - 0.5f / scaleFactor, 0.5f - 0.5f / scaleFactor);
        Vector2 uv1 = new Vector2(0.5f + 0.5f / scaleFactor, 0.5f + 0.5f / scaleFactor);
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, baseSize, uv0, uv1))
        {
            C.ShowBlacksmith = !C.ShowBlacksmith;
            C.Save();
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltipText);
            ImGui.Text($"Showing Leves: {state}");
            ImGui.EndTooltip();
        }
        if (sameLine)
            ImGui.SameLine();
    } //

    #endregion

    private void DrawButtonStar(string tooltip, ref bool state, bool sameLine)
    {
        var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();

        // gold/enabled position
        Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
        Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);

        // silver/disabled position
        Vector2 uvMin2 = new Vector2(0.3611f, 0.0417f);
        Vector2 uvMax2 = new Vector2(0.6389f, 0.4583f);

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));

        if (state)
        {
            if (ImGui.ImageButton(starTex.ImGuiHandle, new Vector2(24, 24), uvMin, uvMax))
            {
                state = !state;
                C.Save();
            }
        }
        else if (!state)
        {
            if (ImGui.ImageButton(starTex.ImGuiHandle, new Vector2(24, 24), uvMin2, uvMax2))
            {
                state = !state;
                C.Save();
            }
        }
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(tooltip);
        if (sameLine)
            ImGui.SameLine();
    }

    private void DrawLeveStatusButton()
    {
        ISharedImmediateTexture? icon = null;

        icon = LeveStatusDict[C.CompleteFilter];

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));

        float scaleFactor = 1.2f;
        Vector2 baseSize = new Vector2(24, 24);
        Vector2 uv0 = new Vector2(0.5f - 0.5f / scaleFactor, 0.5f - 0.5f / scaleFactor);
        Vector2 uv1 = new Vector2(0.5f + 0.5f / scaleFactor, 0.5f + 0.5f / scaleFactor);
        if (ImGui.ImageButton(icon.GetWrapOrEmpty().ImGuiHandle, baseSize, uv0, uv1))
        {
            C.CompleteFilter = C.CompleteFilter + 1;
            if (C.CompleteFilter > 2)
                C.CompleteFilter = 0;
            C.Save();
        }

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();

        string tooltipText = "";

        if (C.CompleteFilter == 0)
        {
            tooltipText = "Showing all leves";
        }
        else if (C.CompleteFilter == 1)
        {
            tooltipText = "Showing Completed Leves";
        }
        else if (C.CompleteFilter == 2)
        {
            tooltipText = "Showing Incomplete Leves";
        }

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.Text(tooltipText);
            ImGui.EndTooltip();
        }
        ImGui.SameLine();

    }

    private void DrawFavStar()
    {
        var starTex = Svc.Texture.GetFromGame("ui/uld/linkshell_hr1.tex").GetWrapOrEmpty();
        Vector2 uvMin = new Vector2(0.027825013f, 0.04166667f);
        Vector2 uvMax = new Vector2(0.305575f, 0.4583333f);

        ImGui.Image(starTex.ImGuiHandle, new(20, 20), uvMin, uvMax);
    }

    private void DrawCompleteLeves()
    {
        var CompleteTexture = LeveStatusDict[1].GetWrapOrEmpty();
        ImGui.Image(CompleteTexture.ImGuiHandle, new(20, 20));
    }

    private void DrawList()
    {
        /*
         * Need to figure out how to filter out leves here in a good manner...
         * Doing it jukka's way is slightly more complicated than what I'm doing (since we went different routes in filtering)
         * Maybe can do a "if (!classEnabled && leve.jobID == job#) might be the best way to do it
        */

        var widthFactor = (ImGui.GetWindowWidth() - 10) / 10;
        ImGui.BeginGroup();
        if (C.OnlyFavorites)
        {
            ImGui.Text($"Showing: {C.FavoriteLeves.Count} out of {CrafterLeves.Count}");
        }
        else
        {
            ImGui.Text($"Showing: {VisibleLeves.Count} out of {CrafterLeves.Count}");
        }

        var firstGroupWidth = Math.Max(350, widthFactor * 4f);
        var firstGroupHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 300),
                                           new Vector2(firstGroupWidth, firstGroupHeight));
        ImGui.BeginChild("###LeveList", new Vector2(0), true, ImGuiWindowFlags.NavFlattened | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        foreach (var kdp in CrafterLeves)
        {
            if (FilterLeve(kdp.Key))
            {
                var id = (int)kdp.Key;

                ImGui.PushID(id);
                if (ImGui.Selectable($"###Leve{id}", selectedLeve == id, ImGuiSelectableFlags.SpanAllColumns))
                    selectedLeve = id;
                if (ImGui.IsItemHovered())
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (!C.workList.Any(e => e.LeveID == id))
                        {
                            C.workList.Add(new LeveEntry { LeveID = kdp.Key, InputValue = 1, ItemAmount = 0 });
                        }
                    }

                }

                ImGui.SetItemAllowOverlap();
                ImGui.SameLine();

                if (C.FavoriteLeves.Contains(kdp.Key))
                {
                    DrawFavStar();
                    ImGui.SameLine(0, 5);
                }
                if (IsComplete(kdp.Key))
                {
                    DrawCompleteLeves();
                    ImGui.SameLine(0, 5);
                }
                if (LeveTypeDict[kdp.Value.JobAssignmentType].AssignmentIcon != null)
                {
                    ImGui.Image(LeveTypeDict[kdp.Value.JobAssignmentType].AssignmentIcon.GetWrapOrEmpty().ImGuiHandle, new(20, 20));
                    ImGui.SameLine(0, 5);
                    ImGui.AlignTextToFramePadding();
                }
                ImGui.Text($"[{kdp.Value.Level}] {kdp.Value.LeveName}");
                ImGui.PopID();
            }
        }

        ImGui.EndChild();
        ImGui.EndGroup();

        ImGui.SameLine();

        ImGui.BeginGroup();
        ImGui.Text("Selected Leve Details");

        var secondGroupWidth =
            firstGroupWidth > 300 ? Math.Max(300, widthFactor * 6) - 25 : ImGui.GetWindowWidth() - 330;
        ImGui.SetNextWindowSizeConstraints(new Vector2(200f, 300),
                                           new Vector2(secondGroupWidth, firstGroupHeight));
        ImGui.BeginChild("###LeveDetail", new Vector2(0), true, ImGuiWindowFlags.NavFlattened | ImGuiWindowFlags.NoScrollbar);

        DrawLeveDetails();

        ImGui.EndChild();
        ImGui.EndGroup();
    }

    private bool FilterLeve(uint leveId)
    {
        var showLeve = true;
        var jobAssignmentType = CrafterLeves[leveId].JobAssignmentType;

        if (C.OnlyFavorites)
        {
            return C.FavoriteLeves.Contains(leveId);
        }

        if (C.LevelFilter > 0)
        {
            showLeve &= CrafterLeves[leveId].Level == C.LevelFilter;
        }

        if (C.CompleteFilter == 1)
        {
            showLeve &= IsComplete(leveId);
        }
        else if (C.CompleteFilter == 2)
        {
            showLeve &= !IsComplete(leveId);
        }

        if (!string.IsNullOrEmpty(C.NameFilter))
        {
            showLeve &= CrafterLeves[leveId].LeveName.Contains(C.NameFilter, StringComparison.OrdinalIgnoreCase);
        }

        // Option #1 (one I created becuase I might be dumb
        // showLeve &= showLeve && JobFilter(LeveDict[leveId].JobID);

        // Option #2 (One Jukka made, might work better...)
        // post ice edit: yes it does XD
        showLeve &= showLeve && !C.GetJobFilter().Contains(jobAssignmentType);

        if (VisibleLeves.Contains(leveId) && !showLeve)
        {
            VisibleLeves.Remove(leveId);
        }
        else if (!VisibleLeves.Contains(leveId) && showLeve)
        {
            VisibleLeves.Add(leveId);
        }

        return showLeve;
    }

    private void DrawLeveDetails()
    {
        var leve = (uint)selectedLeve;
        var questSheet = Svc.Data.GetExcelSheet<Quest>();
        if (CrafterLeves.ContainsKey(leve))
        {
            // string NPCName = NPCResidentsheet.GetRow(NPC).Singular.ToString();
            ImGui.Text($"LeveID: {leve}");
            ImGui.Text($"[{CrafterLeves[leve].Level}] {CrafterLeves[leve].LeveName}");
            ImGui.Separator();
            ImGui.Text($"EXP Reward: {CrafterLeves[leve].ExpReward:N0}");
            ImGui.Text($"Gil Reward: {CrafterLeves[leve].GilReward:N0} ± 5%%");
            ImGui.Separator();
            var vendorId = CrafterLeves[leve].LeveVendorID;
            var startZoneId = LeveNPCDict[vendorId].ZoneID;
            ImGui.Text($"Starting Zone: {ZoneName(startZoneId)}");
            if (ImGui.Selectable($"NPC: {CrafterLeves[leve].LeveVendorName}"))
            {
                var flagX = LeveNPCDict[vendorId].flagX;
                var flagZ = LeveNPCDict[vendorId].flagZ;
                SetFlagForNPC(startZoneId, flagX, flagZ);
            }
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.Text("Click to flag map Location");
                ImGui.EndTooltip();
            }
            ImGui.SameLine();
            FontAwesome.Print(ImGuiColors.DalamudWhite, FontAwesomeIcon.Flag);
            
            uint turninNPCId = CrafterLeves[leve].LeveTurninVendorID;
            string turninName = string.Empty;
            if (turninNPCId != 0)
            {
                turninName = NPCName(turninNPCId);
            }
            else
            {
                turninName = "not valid";
            }
            ImGui.Text($"Turnin NPC: {turninName}");
            ImGui.Text($"Is Complete:");
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            if (IsComplete(leve))
            {
                var CompleteTexture = LeveStatusDict[1].GetWrapOrEmpty();
                ImGui.Image(CompleteTexture.ImGuiHandle, new(24, 24));
            }
            else
            {
                var CompleteTexture = LeveStatusDict[2].GetWrapOrEmpty();
                ImGui.Image(CompleteTexture.ImGuiHandle, new(24, 24));
            }
            if (IsStarted(leve))
            {
                ImGui.Text("Quest is Accepted and Started");
            }
            ImGui.Separator();
            ImGui.Text($"Required Items:");
            if (CrafterLeves[leve].RepeatAmount > 1)
            {
                var totalTurnin = CrafterLeves[leve].RepeatAmount;
                ImGui.Text($"    {CrafterLeves[leve].TurninAmount}({totalTurnin})x {CrafterLeves[leve].ItemName}");
            }
            else
            {
                ImGui.Text($"    {CrafterLeves[leve].TurninAmount}x {CrafterLeves[leve].ItemName}");
            }
            ImGui.SameLine(0, 10);
            ImGui.Image(CrafterLeves[leve].ItemIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(20));
            ImGui.Separator();
            ImGui.PushID((int)leve);
            if (ImGui.Button(C.FavoriteLeves.Contains(leve) ? $"Remove from Favorites" : "Add to favorites"))
            {
                if (C.FavoriteLeves.Contains(leve))
                {
                    C.FavoriteLeves.Remove(leve);
                    C.Save();
                }
                else
                {
                    C.FavoriteLeves.Add(leve);
                    C.Save();
                }
            }
            var requiredQuestId = LeveNPCDict[vendorId].RequiredQuestId;
            bool levesQuestUnlocked = QuestChecker(requiredQuestId);

            using (ImRaii.Disabled(!levesQuestUnlocked))
            {
                if (C.workList.Any(e => e.LeveID == leve))
                {
                    if (ImGui.Button("Remove from WorkList"))
                    {
                        C.workList.RemoveAll(e => e.LeveID == leve);
                        C.Save();
                    }
                }
                else if (!C.workList.Any(e => e.LeveID == leve))
                {
                    if (ImGui.Button("Add to WorkList"))
                    {
                        C.workList.Add(new LeveEntry { LeveID = leve, InputValue = 1, ItemAmount = 0 });
                        C.Save();
                    }
                }
            }
            if (!levesQuestUnlocked)
            {
                ImGui.SameLine();
                FontAwesome.Print(ImGuiColors.DalamudRed, FontAwesome.Cross);
                if (ImGui.IsItemHovered())
                {
                    bool LimsaLeveNPC = IsMSQComplete(66005); // Just Deserts
                    bool UlDahLeveNPC = IsMSQComplete(65856); // Way down in the hole
                    bool GridaniaNPC = IsMSQComplete(65665); // Spirithold Broken

                    ImGui.BeginTooltip();
                    ImGui.Text($"You need to complete the following quest(s) to be able to do this leve");
                    if (requiredQuestId == 0)
                    {
                        uint questId = 0;

                        if (LimsaLeveNPC)
                            questId = 66229;
                        else if (UlDahLeveNPC)
                            questId = 66223;
                        else if (GridaniaNPC)
                            questId = 65756;

                        var questName = questSheet.GetRow(questId).Name.ToString();
                        ImGui.Text(questName);
                    }
                    else if (requiredQuestId == 1)
                    {
                        uint questId1 = 0;
                        uint questId2 = 0;

                        if (LimsaLeveNPC)
                        {
                            questId1 = 66229;
                            questId2 = 65595;
                        }
                        else if (UlDahLeveNPC)
                        {
                            questId1 = 66223;
                            questId2 = 65594;
                        }
                        else if (GridaniaNPC)
                        {
                            questId1 = 65756;
                            questId2 = 65596;
                        }

                        var quest1Name = questSheet.GetRow(questId1).Name.ToString();
                        var quest2Name = questSheet.GetRow(questId2).Name.ToString();

                        if (!IsMSQComplete(questId1))
                        {
                            ImGui.Text($"  -> {quest1Name}");
                        }
                        if (!IsMSQComplete(questId2))
                        {
                            ImGui.Text($"  -> {quest2Name}");
                        }
                    }
                    else
                    {
                        var questName = questSheet.GetRow(requiredQuestId).Name.ToString();
                        ImGui.Text(questName);
                    }
                    ImGui.EndTooltip();
                }
            }
            ImGui.PopID();
        }
        else
        {
            ImGui.Text("No Leve Selected");
        }

    }


}
