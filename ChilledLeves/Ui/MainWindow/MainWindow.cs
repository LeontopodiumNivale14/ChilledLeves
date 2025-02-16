using ChilledLeves.Scheduler;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using ECommons.Configuration;
using FFXIVClientStructs.FFXIV.Common.Lua;
using Lumina.Data.Parsing;
using Lumina.Excel.Sheets;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using static Dalamud.Interface.Utility.Raii.ImRaii;

namespace ChilledLeves.Ui.MainWindow;

internal class MainWindow : Window
{
    private static int selectedLeve;

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

    public override void Draw()
    {
        ImGui.SetNextItemWidth(100);
        ImGui.InputInt("Temp for synth", ref SchedulerMain.SynthRunAmount);
        ImGui.SameLine();
        if (ImGui.Button("Start"))
        {
            SchedulerMain.EnablePlugin();
        }
        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            SchedulerMain.DisablePlugin();
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
        ImGui.Dummy(new Vector2(0, 20));
        ImGui.Text($"Allowances: {Allowances}/100");
        ImGui.Text($"Next 3 in: {NextAllowances:hh':'mm':'ss}");
        ImGui.Spacing();
        if (ImGui.Button("Open Worklist"))
        {
            P.settingWindow.IsOpen = !P.settingWindow.IsOpen;
        }
        ImGui.EndGroup();
        ImGui.Separator();

        DrawList();
    }

    private void DrawFilterPanel()
    {
        ImGui.Text("Filter Leves");

        DrawButtonStar("Show only favorites", ref C.LeveFilter.OnlyFavorites, true);
        DummyButton(8);
        DrawButton(5, $"Leves", ref C.LeveFilter.ShowCarpenter, true);
        DrawButton(6, $"Leves", ref C.LeveFilter.ShowBlacksmith, true);
        DrawButton(7, $"Leves", ref C.LeveFilter.ShowArmorer, true);
        DrawButton(8, $"Leves", ref C.LeveFilter.ShowGoldsmith, true);
        DrawButton(9, $"Leves", ref C.LeveFilter.ShowLeatherworker, true);
        DrawButton(10, $"Leves", ref C.LeveFilter.ShowWeaver, true);
        DrawButton(11, $"Leves", ref C.LeveFilter.ShowAlchemist, true);
        DrawButton(12, $"Leves", ref C.LeveFilter.ShowCulinarian, false);

        ImGui.Text("Level:");
        ImGui.AlignTextToFramePadding();
        ImGui.SameLine();
        var level = C.LeveFilter.LevelFilter > 0 ? C.LeveFilter.LevelFilter.ToString() : "";
        ImGui.SetNextItemWidth(30);
        if (ImGui.InputText("###Level", ref level, 3))
        {
            C.LeveFilter.LevelFilter = (int)(uint.TryParse(level, out var num) && num > 0 ? num : 0);
            C.Save();
        }
        ImGui.SameLine();
        ImGui.Text("Name:");
        ImGui.AlignTextToFramePadding();
        ImGui.SameLine();
        ImGui.SetNextItemWidth(220);
        if (ImGui.InputText("###Name", ref C.LeveFilter.NameFilter, 200))
        {
            C.LeveFilter.NameFilter = C.LeveFilter.NameFilter.Trim();
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

    private void DrawButton(uint row, string tooltip, ref bool state, bool sameLine)
    {
        ISharedImmediateTexture? icon = LeveTypeDict[row].AssignmentIcon;
        string tooltipText = $"{LeveTypeDict[row].LeveClassType} {tooltip}";

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
            ImGui.SetTooltip(tooltipText);
        if (sameLine)
            ImGui.SameLine();
    }

    private void DrawButtonStar(string tooltip, ref bool state, bool sameLine)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0));
        ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));
        if (Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), SilverStarImage).TryGetWrap(out var silverTexture, out var _b))
        {
            if (ImGui.ImageButton(silverTexture.ImGuiHandle, new Vector2(24)))
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

    private void DrawFavStar()
    {
        if (Svc.Texture.GetFromManifestResource(Assembly.GetExecutingAssembly(), GoldStarImage).TryGetWrap(out var goldTexture, out var _b))
        {
            ImGui.Image(goldTexture.ImGuiHandle, new(20, 20));
        }
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
        if (C.LeveFilter.OnlyFavorites)
        {
            ImGui.Text($"Showing: {C.FavoriteLeves.Count} out of {CrafterLeves.Count}");
        }
        else
        {
            ImGui.Text($"Showing: {VisibleLeves.Count} out of {CrafterLeves.Count}");
        }

        var firstGroupWidth = Math.Max(350, widthFactor * 4f);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0, 300),
                                           new Vector2(firstGroupWidth, 300));
        ImGui.BeginChild("###LeveList", new Vector2(0), true, ImGuiWindowFlags.NavFlattened | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        foreach (var kdp in CrafterLeves)
        {
            if (FilterLeve(kdp.Key))
            {
                var id = (int) kdp.Key;

                ImGui.PushID(id);
                if (ImGui.Selectable($"###Leve{id}", selectedLeve == id, ImGuiSelectableFlags.SpanAllColumns))
                    selectedLeve = id;
                if (ImGui.IsItemHovered())
                {
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (!C.workList.Any(e => e.LeveID == id))
                        {
                            C.workList.Add(new LeveEntry { LeveID = kdp.Key, InputValue = 0});
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
                                           new Vector2(secondGroupWidth, 300));
        ImGui.BeginChild("###LeveDetail", new Vector2(0), true, ImGuiWindowFlags.NavFlattened);

        DrawLeveDetails();

        ImGui.EndChild();
        ImGui.EndGroup();
    }

    private bool FilterLeve(uint leveId)
    {
        var showLeve = true;
        var jobAssignmentType = CrafterLeves[leveId].JobAssignmentType;

        if (C.LeveFilter.OnlyFavorites)
        {
            return C.FavoriteLeves.Contains(leveId);
        }

        if (C.LeveFilter.LevelFilter > 0)
        {
            showLeve &= CrafterLeves[leveId].Level == C.LeveFilter.LevelFilter;
        }

        if (!string.IsNullOrEmpty(C.LeveFilter.NameFilter))
        {
            showLeve &= CrafterLeves[leveId].LeveName.Contains(C.LeveFilter.NameFilter, StringComparison.OrdinalIgnoreCase);
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
        var NPCResidentsheet = Svc.Data.GetExcelSheet<ENpcResident>();
        if (CrafterLeves.ContainsKey(leve))
        {
            // string NPCName = NPCResidentsheet.GetRow(NPC).Singular.ToString();
            ImGui.Text($"[{CrafterLeves[leve].Level}] {CrafterLeves[leve].LeveName}");
            ImGui.Separator();
            ImGui.Text($"EXP Reward: {CrafterLeves[leve].ExpReward:N0}");
            ImGui.Text($"Gil Reward: {CrafterLeves[leve].GilReward:N0} ± 5%%");
            ImGui.Separator();
            ImGui.Text($"Starting Zone: ");
            ImGui.Text($"NPC: {CrafterLeves[leve].LeveVendorName}");
            ImGui.Text($"Is Complete: {IsComplete(leve)}");
            ImGui.Text($"Is Accepted: {IsAccepted(leve)}");
            ImGui.Separator();
            ImGui.Text($"Required Items:");
            ImGui.Text($"    {CrafterLeves[leve].TurninAmount}x {CrafterLeves[leve].ItemName}");
            ImGui.SameLine(0, 10);
            ImGui.Image(CrafterLeves[leve].ItemIcon.GetWrapOrEmpty().ImGuiHandle, new Vector2(20));
            ImGui.Separator();
            ImGui.PushID((int)leve);
            if (ImGui.Button(C.FavoriteLeves.Contains(leve) ? $"Remove from Favorites" : "Add to favorites"))
            {
                if (C.FavoriteLeves.Contains(leve))
                {
                    C.FavoriteLeves.Remove(leve);
                }
                else
                {
                    C.FavoriteLeves.Add(leve);
                }
            }
            if (C.workList.Any(e => e.LeveID == leve))
            {
                if (ImGui.Button("Remove from workshop"))
                {
                    C.workList.RemoveAll(e => e.LeveID == leve);
                    C.Save();
                }
            }
            else if (!C.workList.Any(e => e.LeveID == leve))
            {
                if (ImGui.Button("Add to workshop"))
                {
                    C.workList.Add(new LeveEntry { LeveID = leve, InputValue = 0 });
                    C.Save();
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
